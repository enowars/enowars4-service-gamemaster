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
    async def __init__(self, address: str, port: int, logger: Logger) -> None:
        headers = {"user-agent": random_useragent()}
        self.http_session = await aiohttp.ClientSession(headers=headers)
        self.address = address # type: str
        self.port = port # type: int
        self.logger = logger
    async def __del__(self):
        await self.http_session.close()

    async def login(self, username: str, password: str) ->None:
        params = {'username': username, 'password': password}
        resp = await self.http_session.request("POST", self.scheme + "://" + self.address + ":" + str(self.port) + "/api/account/login", params=params)

    def http_post(self, route: str = "/", params: Optional[Dict] = None, port: Optional[int] = None, scheme: str = "http",
                  timeout: Optional[int] = None, **kwargs) -> requests.Response:
        """
        Performs a (http) requests.post to the current host.
        Caches cookies in self.http_session
        :param route: The route
        :param params: The parameter
        :param port: The remote port in case it has not been specified at creation
        :param scheme: The scheme (defaults to http)
        :param timeout: How long we'll try to connect
        :return: The response
        """
        kwargs.setdefault('allow_redirects', True)
        return self.http("post", route, params, port, scheme, timeout, **kwargs)

    def http_get(self, route: str = "/", params: Optional[Dict] = None, port: Optional[int] = None, scheme: str = "http",
                 timeout: Optional[int] = None, **kwargs) -> requests.Response:
        """
        Performs a (http) requests.get to the current host.
        Caches cookies in self.http_session
        :param params: The parameter
        :param route: The route
        :param port: The remote port in case it has not been specified at creation
        :param scheme: The scheme (defaults to http)
        :param timeout: How long we'll try to connect
        :return: The response
        """
        with aiohttp.Timeout(5):
            resp =  yield from http_session.get(scheme + "://" + self.address + ":" + str(port) + route)
            try:
                
                return (yield from resp.text())
            except Exception as e:
                # .close() on exception.
                resp.close()
                raise e
            finally:
                # .release() otherwise to return connection into free connection pool.
                # It's ok to release closed response:
                # https://github.com/KeepSafe/aiohttp/blob/master/aiohttp/client_reqrep.py#L664
                yield from resp.release()


    async def http(self, method: str, route: str = "/", params: Optional[Dict] = None, port: Optional[int] = None, scheme: str = "http",
             timeout: Optional[int] = None, **kwargs) -> requests.Response:
        """
        Performs an http request (requests lib) to the current host.
        Caches cookies in self.http_session
        :param method: The request method
        :param params: The parameter
        :param route: The route
        :param port: The remote port in case it has not been specified at creation
        :param scheme: The scheme (defaults to http)
        :param timeout: How long we'll try to connect (default: self.timeout)
        :return: The response
        """

        resp = await self.http_session.request(method, scheme + "://" + self.address + ":" + str(port) + route, params=params, timeout=timeout, **kwargs)
        return resp
