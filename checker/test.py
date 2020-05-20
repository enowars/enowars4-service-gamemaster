from gamemasterlib import *
import logging

logger = logging.getLogger()
handler = logging.StreamHandler(sys.stdout)
#handler.setFormatter(ELKFormatter("%(message)s")) #ELK-ready output
logger.addHandler(handler)
logger.setLevel(logging.DEBUG)
tmp = http_interface("127.0.0.1", "8001", logger)
tmp.login("test", "test")