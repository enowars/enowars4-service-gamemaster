import time
import logging
import sys
import aiohttp
import random
import string
import json
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
    
    def get_username():
        return german_faker.first_name() + german_faker.last_name() + ''.join(random.choice(string.digits) for _ in range(10))

    def getpassword(username:str)->str:
        return hash(username+"secret")

    async def putflag(self, logger: LoggerAdapter, task: CheckerTaskMessage, collection: MotorCollection) -> None:
        username = get_username()
        await collection.insert_one({ 'flag' : task.flag, 'username': username })
        password = getpassword (username)
        logger.debug("Putting Flag...")
        

    async def getflag(self, logger: LoggerAdapter, task: CheckerTaskMessage, collection: MotorCollection) -> None:


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
