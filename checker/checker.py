import time
import logging
import sys
import aiohttp
import asyncio
import random
import string
from hashlib import sha256
from gamemasterlib import *
from enochecker_async import BaseChecker, BrokenServiceException, create_app, OfflineException, ELKFormatter, CheckerTaskMessage
from logging import LoggerAdapter
from motor import MotorCollection
from faker import Faker

class GamemasterChecker(BaseChecker):
    port = 8001

    def __init__(self):
        super(GamemasterChecker, self).__init__("Gamemaster", 8080, 2, 1, 1)
        self.german_faker = Faker('de_DE')
    
    def getusername(self):
        return self.german_faker.first_name() + self.german_faker.last_name() + ''.join(random.choice(string.digits) for _ in range(10))
    def getpassword(self, username:str)->str:
        #return sha256(username.encode('utf-8')+"suchsecretmuchwow".encode('utf-8')).hexdigest()
        return "password"
    def getemail(self, username:str)->str:
        return self.german_faker.free_email()

    async def createmasterandput(self, logger: LoggerAdapter, flag: str, address: str, collection: MotorCollection, clients:dict) -> (str, str):
        username = self.getusername()
        password = self.getpassword (username)
        email = self.getemail(username)
        logger.debug("Putting Flag...")
        interface : HttpInterface = await HttpInterface.setup(address, GamemasterChecker.port, logger)
        await interface.register(username, email, password)
        clients[username] = password
        sessionname = self.german_faker.pystr()
        notes = flag
        password = self.getpassword(sessionname)
        await collection.insert_one({ 'flag' : flag, 'username': username, 'session': sessionname })
        response : aiohttp.ClientResponse = await interface.create_session(sessionname, notes, password)
        sessionid = (await response.json())['id']
        await interface.close()
        return username, sessionid

    async def createuser(self, logger: LoggerAdapter, address: str, collection: MotorCollection, clients:dict) -> str:
        username = self.getusername()
        password = self.getpassword (username)
        email = self.getemail(username)
        interface : HttpInterface = await HttpInterface.setup(address, GamemasterChecker.port, logger)
        await interface.register(username, email, password)
        logger.debug (f"Inserted {username}")
        clients[username] = password
        await interface.close()
        return username

    async def useraddsession(self, logger: LoggerAdapter, address:str, clients:dict, sessionid:int,mastername:str, collection: MotorCollection) -> None:
        interface : HttpInterface = await HttpInterface.setup(address, GamemasterChecker.port, logger)
        await interface.login(mastername, self.getpassword(mastername))
        await asyncio.gather(*[interface.add_to_session(sessionid, k) for k in clients.keys()])
        await interface.close()

    async def clienttodb(self, logger:LoggerAdapter, round:int, team:int, collection:MotorCollection, clients:dict):
        for k, v in clients.items():
            await collection.insert_one({ 'username' : k, 'password': v, 'round': round, 'team': team})

    async def dbtoclient(self, logger:LoggerAdapter, round:int, team:int, collection:MotorCollection) -> dict:
        cursor = collection.find({ 'round': round-1, 'team': team})
        result = {}
        for document in await cursor.to_list(length=100):
            result[document['username']] = document['password']
        return result

    async def putflag(self, logger: LoggerAdapter, task: CheckerTaskMessage, collection: MotorCollection) -> None:
        clients = await self.dbtoclient (logger, task.round, task.teamId, collection)
        for k in list(clients.keys()):
            if bool(random.getrandbits(1)):
                del clients[k]
        mastername, Sessionid = await self.createmasterandput(logger, task.flag, task.address, collection, clients)
        newsize = random.randrange(4, 8, 1)
        logger.debug (f"After Random: len:{len(clients)}, lenkeys:{len(clients.keys())}")
        users_to_create = max(newsize - len(clients), 0)
        if users_to_create > 0:
            await asyncio.gather(
                *[self.createuser(logger, task.address, collection, clients) for i in range(users_to_create)]
                )
        logger.debug(f"{users_to_create} Users added for round {task.round}")
        await self.useraddsession(logger, task.address, clients, Sessionid, mastername, collection)
        await self.clienttodb(logger, task.round, collection, clients)

    async def getflag(self, logger: LoggerAdapter, task: CheckerTaskMessage, collection: MotorCollection) -> None:
        try:
            result = await collection.find_one({ 'flag': task.flag })
            username = result['username']
            session = result['session']
        except:
            raise BrokenServiceException(f"Cannot find flag in Database - likely putflag failed")
        interface : HttpInterface = await HttpInterface.setup(task.address, GamemasterChecker.port, logger)
        await interface.login(username, self.getpassword(username))
        await interface.close()

    async def putnoise(self, logger: LoggerAdapter, task: CheckerTaskMessage, collection: MotorCollection) -> None:
        pass

    async def getnoise(self, logger: LoggerAdapter, task: CheckerTaskMessage, collection: MotorCollection) -> None:
        pass

    async def havoc(self, logger: LoggerAdapter, task: CheckerTaskMessage, collection: MotorCollection) -> None:
        pass

logger = logging.getLogger()
handler = logging.StreamHandler(sys.stdout)
#handler.setFormatter(ELKFormatter("%(message)s")) #ELK-ready output
logger.addHandler(handler)
logger.setLevel(logging.DEBUG)
app = create_app(GamemasterChecker()) # mongodb://mongodb:27017