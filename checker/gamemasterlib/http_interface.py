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
    @property
    def http_useragent(self) -> str:
        """
        The useragent for http(s) requests
        :return: the current useragent
        """
        return self.http_session.headers["User-Agent"]

    @http_useragent.setter
    def http_useragent(self, useragent: str) -> None:
        """
        Sets the useragent for http requests.
        Randomize using http_useragent_randomize()
        :param useragent: the useragent
        """
        self.http_session.headers["User-Agent"] = useragent

    def http_useragent_randomize(self) -> None:
        """
        Choses a new random http useragent.
        Note that http requests will be initialized with a random user agent already.
        To retrieve a random useragent without setting it, use random instead.
        :return: the new useragent
        """
        new_agent = random_useragent()
        self.http_useragent = new_agent
        return new_agent

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
        kwargs.setdefault('allow_redirects', True)
        return self.http("get", route, params, port, scheme, timeout, **kwargs)

    def http(self, method: str, route: str = "/", params: Optional[Dict] = None, port: Optional[int] = None, scheme: str = "http",
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
        if port is None:
            port = self.port
        self.logger.debug(method + " " + scheme + "://" + self.address + ":" + str(port) + route)
        resp = self.http_session.request(method, scheme + "://" + self.address + ":" + str(port) + route, params=params, timeout=timeout, **kwargs)
        resp.raise_for_status()
        return resp

    def ws_connect(self, uri: str) -> None:
        self._websocket_client = websocket.WebSocket()
        cookies = ""
        for cookie in self.http_session.cookies:
            cookies = cookies + cookie.name+"="+cookie.value+";"
        self.logger.debug("ws_connect " + str(uri))
        self._websocket_client.connect(uri, cookie=cookies, timeout=15)

    def ws_recv_frame(self) -> str:
        data = self._websocket_client.recv_frame().data
        self.logger.debug("ws_recv_frame " + str(data))
        return data

    def ws_send(self, data: str) -> None:
        self.logger.debug("ws_send " + str(data))
        return self._websocket_client.send(data)
