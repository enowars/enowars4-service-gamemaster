from typing import Optional, Dict
import aiohttp
import json
import random
import string
import sys
import time
import os
from logging import Logger
from urllib.parse import urlparse
from .user_agents import random_useragent

class http_interface:
    def __init__(self, address: str, port: int, logger: Logger) -> None:
        self.address = address # type: str
        self.port = port # type: int
        self.logger = logger
    async def __del__(self):
        await self.http_session.close()
    @staticmethod
    async def setup(address: str, port: int, logger: Logger):
        tmp = http_interface(address, port, logger)
        tmp.headers = {"user-agent": random_useragent()}
        tmp.http_session = await aiohttp.ClientSession(headers=tmp.headers)
        return tmp

    async def login(self, username: str, password: str) ->None:
        params = {'username': username, 'password': password}
        resp = await self.http_session.request("POST", self.scheme + "://" + self.address + ":" + str(self.port) + "/api/account/login", params=params)