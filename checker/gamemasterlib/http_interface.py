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
from enochecker_async import BrokenServiceException, OfflineException

class HttpInterface:
    def __init__(self, address: str, port: int, logger: Logger, httpsession: aiohttp.ClientSession) -> None:
        self.address = address 
        self.port = port
        self.logger = logger
        self.http_session = httpsession
        self.scheme = "http"
    async def __del__(self):
        await self.http_session.close()
    @staticmethod
    async def setup(address: str, port: int, logger: Logger):
        http_session = aiohttp.ClientSession(headers={"user-agent": random_useragent()})   #, 'Content-Type': 'multipart/form-data;'
        return HttpInterface(address, port, logger, http_session)

    async def register(self, username: str, email: str, password: str) ->aiohttp.ClientResponse:
        try:
            params = {'username': username, 'email':email, 'password': password}
            response:aiohttp.ClientResponse = await self.http_session.request("POST", self.scheme + "://" + self.address + ":" + str(self.port) + "/api/account/register", data=params)
        except:
            raise OfflineException()
        if response.status!=200:
            raise BrokenServiceException(f"Register Failed: {response}")
    async def login(self, username: str, password: str) ->aiohttp.ClientResponse:
        try:
            params = {'username': username, 'password': password}
            response:aiohttp.ClientResponse = await self.http_session.request("POST", self.scheme + "://" + self.address + ":" + str(self.port) + "/api/account/login", data=params)
        except:
            raise OfflineException()
        if response.status!=200:
            raise BrokenServiceException(f"Login Failed: {response}")
    async def create_session(self, name:str, notes:str, password:str):
        try:
            params = {'name': name, 'notes': notes, 'password': password}
            response:aiohttp.ClientResponse = await self.http_session.request("POST", self.scheme + "://" + self.address + ":" + str(self.port) + "/api/session/create", data=params)
        except:
            raise OfflineException()
        if response.status!=200:
            raise BrokenServiceException(f"Create Session Failed: {response}")