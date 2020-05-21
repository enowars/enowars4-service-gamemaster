import time
import logging
import sys
import aiohttp
import random
import string
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
        self.clients = {}
    
    def getusername(self):
        return self.german_faker.first_name() + self.german_faker.last_name() + ''.join(random.choice(string.digits) for _ in range(10))

    def getpassword(self, username:str)->str:
        return hash(username+"suchsecretmuchwow")
    def getemail(self, username:str)->str:
        return self.german_faker.free_email()

    async def createmasterandput(self, logger: LoggerAdapter, flag: str, address: str, collection: MotorCollection) -> None:
        username = self.getusername()
        password = self.getpassword (username)
        email = self.getemail(username)
        logger.debug("Putting Flag...")
        interface : HttpInterface = await HttpInterface.setup(address, GamemasterChecker.port, logger)
        await interface.register(username, email, password)
        self.clients[username] = password
        sessionname = self.german_faker.pystr()
        notes = flag
        password = self.getpassword(sessionname)
        await collection.insert_one({ 'flag' : flag, 'username': username, 'session': sessionname })
        response : aiohttp.ClientResponse = await interface.create_session(sessionname, notes, password)
    async def createuser (self, logger: LoggerAdapter, address: str, collection: MotorCollection) -> str:
        username = self.getusername()
        password = self.getpassword (username)
        email = self.getemail(username)
        logger.debug("Create User...")
        interface : HttpInterface = await HttpInterface.setup(address, GamemasterChecker.port, logger)
        await interface.register(username, email, password)
        self.clients[username] = password
        return username
    async def useraddsession (self, logger: LoggerAdapter, address:str, user:str, sessionid:int,mastername:str, collection: MotorCollection) -> None:
        interface : HttpInterface = await HttpInterface.setup(address, GamemasterChecker.port, logger)
        await interface.login(mastername, self.getpassword(mastername))
        await interface.add_to_session(sessionid, user)

    async def putflag(self, logger: LoggerAdapter, task: CheckerTaskMessage, collection: MotorCollection) -> None:
#        cursor = collection.find({'i': {'$lt': 5}}).sort('i')
#...     for document in await cursor.to_list(length=100):
#...         pprint.pprint(document)

#### 
        await self.createmasterandput(logger, task.flag, task.address, collection)
        for k,v in self.clients:
            if bool(random.getrandbits(1)):
                del clients[k]
        newsize = random.randrange(8, 15, 1)
        while self.clients.len()<newsize:
            createuser (logger, task.address, collection)
        

    async def getflag(self, logger: LoggerAdapter, task: CheckerTaskMessage, collection: MotorCollection) -> None:
        try:
            result = await collection.find_one({ 'flag': task.flag })
            username = result['username']
            session = result['session']
        except:
            raise BrokenServiceException(f"Cannot find flag in Database - likely putflag failed")
        interface : HttpInterface = await HttpInterface.setup(task.address, GamemasterChecker.port, logger)
        await interface.login(username, self.getpassword(username))
        

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
