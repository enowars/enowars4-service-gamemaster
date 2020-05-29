import time
import logging
import sys
import aiohttp
import asyncio
import random
import string

from pymongo import ASCENDING
from hashlib import sha256
from gamemasterlib import *
from enochecker_async import BaseChecker, BrokenServiceException, create_app, OfflineException, ELKFormatter, CheckerTaskMessage
from logging import LoggerAdapter
from motor import MotorCollection
from faker import Faker
import traceback

class GamemasterChecker(BaseChecker):
    port = 8001

    def __init__(self):
        super(GamemasterChecker, self).__init__("Gamemaster", 8000, 2, 1, 1)
        self.german_faker = Faker('de_DE')
        self.indexCreated = False
    
    def getusername(self):
        return self.german_faker.first_name() + self.german_faker.last_name() + ''.join(random.choice(string.digits) for _ in range(10))
    def getpassword(self, username:str)->str:
        #return sha256(username.encode('utf-8')+"suchsecretmuchwow".encode('utf-8')).hexdigest()
        return "password"
    def getemail(self, username:str)->str:
        return self.german_faker.free_email()


    async def createindex(self, logger: LoggerAdapter, collection: MotorCollection) -> None:
        logger.debug("CreateFlagIndex")
        try:
            await collection.create_index("flag")
            logger.debug("Flag Index created")
            logger.debug("Create Round/Team Index...")
            await collection.create_index([("round", ASCENDING),("team", ASCENDING)])
            logger.debug("Round/Team Index created.")
        except Exception as e:
            #stacktrace = ''.join(traceback.format_exception(None, e, e.__traceback__))
            logger.debug("CreatedIndexFailed")#.format(stacktrace))

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
        logger.debug (f"Creating user {username}")
        password = self.getpassword (username)
        email = self.getemail(username)
        interface : HttpInterface = await HttpInterface.setup(address, GamemasterChecker.port, logger)
        await interface.register(username, email, password)
        clients[username] = password
        logger.debug (f"Inserted {username}")
        await interface.close()
        return username

    async def useraddsession(self, logger: LoggerAdapter, address:str, clients:dict, sessionid:int,mastername:str, collection: MotorCollection) -> None:
        logger.debug(f"trying to add users to session...")
        interface : HttpInterface = await HttpInterface.setup(address, GamemasterChecker.port, logger)
        logger.debug(f"logging in...")
        await interface.login(mastername, self.getpassword(mastername))
        logger.debug(f"Login Successful")
        await asyncio.gather(*[interface.add_to_session(sessionid, k) for k in clients.keys()])
        logger.debug(f"AddToSession Successful")
        await interface.close()

    async def clienttodb(self, logger:LoggerAdapter, round:int, team:int, collection:MotorCollection, clients:dict):
        logger.debug(f"clienttodb startet")
        for k, v in clients.items():
            logger.debug(f"Inserting Client...")
            await collection.insert_one({ 'username' : k, 'password': v, 'round': round, 'team': team})
        logger.debug(f"clienttodb finished")

    async def dbtoclient(self, logger:LoggerAdapter, round:int, team:int, collection:MotorCollection) -> dict:
        if not self.indexCreated:
            await self.createindex(logger,collection)
            self.indexCreated = True
        logger.debug(f"dbtoclient startet")
        cursor = collection.find({ 'round': round-1, 'team': team})
        result = {}
        for document in await cursor.to_list(length=100):
            result[document['username']] = document['password']
        logger.debug(f"dbtoclient finished")
        return result

    async def putflag(self, logger: LoggerAdapter, task: CheckerTaskMessage, collection: MotorCollection) -> None:
        logger.debug (f"Putflag started")
        clients = await self.dbtoclient (logger, task.round, task.teamId, collection)
        logger.debug (f"Clientdb fetched")
        logger.debug (f"Before Random: len:{len(clients)}")
        for k in list(clients.keys()):
            if bool(random.getrandbits(1)):
                del clients[k]
                logger.debug (f"Deleted Client")
        logger.debug (f"After Random: len:{len(clients)}")
        mastername, Sessionid = await self.createmasterandput(logger, task.flag, task.address, collection, clients)
        logger.debug (f"Master Created")
        #newsize = random.randrange(8, 15, 1)
        newsize = random.randrange(4, 8, 1)
        logger.debug (f"Newsize determined: {newsize}")
        users_to_create = max(newsize - len(clients), 0)
        if users_to_create > 0:
            await asyncio.gather(
                *[self.createuser(logger, task.address, collection, clients) for i in range(users_to_create)]
                )
        logger.debug(f"{users_to_create} Users added for round {task.round}, now {len(clients)} users total")
        await self.useraddsession(logger, task.address, clients, Sessionid, mastername, collection)
        logger.debug (f"Saving back Clientdb..")
        await self.clienttodb(logger, task.round, task.teamId, collection, clients)
        logger.debug (f"Putflag finished")

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
        logger.debug (f"TestLogging started")
        interface : HttpInterface = await HttpInterface.setup(task.address, GamemasterChecker.port, logger)
        resp = await interface.test()
        await interface.close()
        logger.debug (f"TestLogging finished, returned {resp}")



logger = logging.getLogger()
handler = logging.StreamHandler(sys.stdout)
handler.setFormatter(ELKFormatter("%(message)s")) #ELK-ready output
logger.addHandler(handler)
logger.setLevel(logging.DEBUG)
app = create_app(GamemasterChecker()) # mongodb://mongodb:27017