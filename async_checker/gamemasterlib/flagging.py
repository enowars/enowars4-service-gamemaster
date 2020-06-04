from gamemasterlib.http_interface import *;
import aiohttp;
import json
import random
import string
import sys
import time
import os
from logging import Logger
from urllib.parse import urlparse
from .user_agents import random_useragent
from gamemasterlib.http_interface import *


class flagging:
    def __init__(self, address: str, port: int, logger: Logger) -> None:
        self.address = address # type: str
        self.port = port # type: int
        self.logger = logger
        self.http_useragent = random_useragent()

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