from io import TextIOWrapper
from typing import Callable, Literal
from zlib import error as Error
from pandas.core.frame import DataFrame
from pathlib import Path
from datetime import datetime as DateTime, time as Time
import baostock as BaoStock
import sys as System
import os as OS
import json as Json


class HiddenPrints:
    def __enter__(self):
        self._original_stdout = System.stdout
        System.stdout = open(OS.devnull, 'w')

    def __exit__(self, exc_type, exc_val, exc_tb):
        System.stdout.close()
        System.stdout = self._original_stdout


priceMapper: Callable[[str], int] = lambda x: int(float(x)*10000)
rateMapper: Callable[[str], int] = lambda x: int(float(x)*1000000)
intMapper: Callable[[str], int] = lambda x: int(x)
timeMapper: Callable[[
    str], str] = lambda time: f"{time[0:4]}-{time[4:6]}-{time[6:8]}T{time[8:10]}:{time[10:12]}:{time[12:14]}.{time[14:]}Z"
rehabilitationMapper: Callable[[
    str], str] = lambda x: "1" if x == "pre" else "2" if x == "post" else "3"

priceKeyMapBase = {
    "open": "opening",
    "close": "closing",
    "high": "highest",
    "low": "lowest",
    "volume": "volume",
    "amount": "turnover",
    "adjustflag": "rehabilitation"
}
priceValueMapBase = {
    "open": priceMapper,
    "close": priceMapper,
    "high": priceMapper,
    "low": priceMapper,
    "volume": intMapper,
    "amount": priceMapper,
    "adjustflag": rehabilitationMapper
}


def getStockInfo(id: str):
    pass


getStockInfoKeyMap = {
    "code": "id",
    "code_name": "name",
    "ipoDate": "listedDate",
    "outDate": "delistedDate",
    "type": "type",
    "industry": "industry",
    "industryClassification": "classification"
}


def getStockList(type: str, date: str):
    pass


getStockListKeyMap = {
    "code": "id",
    "code_name": "name"
}


def getMinutelyPrice(id: str, beginDate: str, endDate: str, frequency: str = "60", rehabilitation: Literal["pre", "post", "none"] = "none"):
    pass


getMinutelyPriceKeyMap = priceKeyMapBase | {
    "time": "time"
}
getMinutelyPriceValueMap = priceValueMapBase | {
    "time": timeMapper
}


def getDailyPrice(id: str, beginDate: str, endDate: str, rehabilitation: Literal["pre", "post", "none"] = "none"):
    pass


getDailyPriceKeyMap = priceKeyMapBase | {
    "date": "date",
    "preclose": "preClosing",
    "turn": "turnoverRate",
    "peTTM": "per",
    "pbMRQ": "pbr",
    "psTTM": "psr",
    "pcfNcfTTM": "pcfr",
    "tradestatus": "stopped",
    "isST": "specialTreatment"
}
getDailyPriceValueMap = priceValueMapBase | {
    "preclose": priceMapper,
    "turn": rateMapper,
    "peTTM": rateMapper,
    "pbMRQ": rateMapper,
    "psTTM": rateMapper,
    "pcfNcfTTM": rateMapper,
    "tradestatus": lambda x: x == "0",
    "isST": lambda x: x == "1"
}


def getWeeklyPrice(id: str, beginDate: str, endDate: str, frequency: str = "week", rehabilitation: Literal["pre", "post", "none"] = "none"):
    pass


getWeeklyPriceKeyMap = priceKeyMapBase | {
    "date": "date",
    "turn": "turnoverRate",
}
getWeeklyPriceValueMap = priceValueMapBase | {
    "turn": rateMapper
}


def getStockInfo(id: str):
    data1 = BaoStock.query_stock_basic(id).get_data()
    data2 = BaoStock.query_stock_industry(id).get_data()
    return {getStockInfoKeyMap[key]: data1.at[0, key] for key in data1.keys() if key in getStockInfoKeyMap} | {getStockInfoKeyMap[key]: data2.at[0, key] for key in data2.keys() if key in getStockInfoKeyMap}


def getStockList(type: str = "default", date: str = ""):
    result = []
    data: DataFrame
    if type == "sz50":
        data = BaoStock.query_sz50_stocks(date).get_data()
    elif type == "hs300":
        data = BaoStock.query_hs300_stocks(date).get_data()
    elif type == "zz500":
        data = BaoStock.query_zz500_stocks(date).get_data()
    elif type == "default":
        path = Path(__file__).parent.absolute()/"list.json"
        file = open(str(path), "r", encoding="utf8")
        return Json.loads(file.read())
    else:
        data = BaoStock.query_stock_basic().get_data()
        keys = data.keys()
        data = [x[1]
                for x in BaoStock.query_stock_basic().get_data().iterrows()]
        if type == "index":
            data = [x for x in data if x["type"] == "2"]
        elif type == "stock":
            data = [x for x in data if x["type"] == "1"]
        for row in data:
            result.append({getStockListKeyMap[key]: row[key]
                          for key in keys if key in getStockListKeyMap})
        return result
    for row in data.iterrows():
        result.append({getStockListKeyMap[key]: row[1][key]
                      for key in data.keys() if key in getStockListKeyMap})
    return result


def getMinutelyPrice(id: str, beginDate: str, endDate: str, frequency: str = "60", rehabilitation: Literal["pre", "post", "none"] = "none"):
    result = []
    rehabilitation = rehabilitation == "1" if rehabilitation == "pre" else "2" if rehabilitation == "post" else "3"
    data = BaoStock.query_history_k_data_plus(
        id, ",".join(getMinutelyPriceKeyMap.keys()), beginDate, endDate, frequency, rehabilitation).get_data()
    for row in data.iterrows():
        dataRow = {getMinutelyPriceKeyMap[key]: getMinutelyPriceValueMap[key](row[1][key]) if key in getMinutelyPriceValueMap else row[1][key]
                   for key in data.keys() if key in getMinutelyPriceKeyMap}
        dataRow["id"] = id
        result.append(dataRow)
    return result


def getDailyPrice(id: str, beginDate: str, endDate: str, rehabilitation: Literal["pre", "post", "none"] = "none"):
    result = []
    rehabilitation = rehabilitation == "1" if rehabilitation == "pre" else "2" if rehabilitation == "post" else "3"
    data = BaoStock.query_history_k_data_plus(
        id, ",".join(getDailyPriceKeyMap.keys()), beginDate, endDate, "d", rehabilitation).get_data()
    for row in data.iterrows():
        dataRow = {getDailyPriceKeyMap[key]: getDailyPriceValueMap[key](row[1][key]) if key in getDailyPriceValueMap else row[1][key]
                   for key in data.keys() if key in getDailyPriceKeyMap}
        dataRow["id"] = id
        result.append(dataRow)
    return result


def getWeeklyPrice(id: str, beginDate: str, endDate: str, frequency: str = "week", rehabilitation: Literal["pre", "post", "none"] = "none"):
    result = []
    rehabilitation = rehabilitation == "1" if rehabilitation == "pre" else "2" if rehabilitation == "post" else "3"
    data = BaoStock.query_history_k_data_plus(
        id, ",".join(getWeeklyPriceKeyMap.keys()), beginDate, endDate, "w" if frequency == "week" else "m", rehabilitation).get_data()
    for row in data.iterrows():
        dataRow = {getWeeklyPriceKeyMap[key]: getWeeklyPriceValueMap[key](row[1][key]) if key in getWeeklyPriceValueMap else row[1][key]
                   for key in data.keys() if key in getWeeklyPriceKeyMap}
        dataRow["id"] = id
        result.append(dataRow)
    return result

startTime = Time(9,30)
endTime = Time(15)
def checkTradeStatus(date: str = ""):
    if date == "" and (DateTime.now().time() < startTime or DateTime.now().time()>endTime):
        return "false"
    return "true" if BaoStock.query_trade_dates(date, date).get_row_data()[1] == "1" else "false"


if __name__ == "__main__":
    with HiddenPrints():
        login = BaoStock.login()
    if login.error_code != "0":
        raise Error("Connection failed")
    System.stdout = TextIOWrapper(System.stdout.buffer, encoding='utf8')
    System.stdin = TextIOWrapper(System.stdin.buffer, encoding='utf8')
    for line in System.stdin:
        args = line.strip().split()
        operation = args[0]
        args = args[1:]
        result: str = ""
        if operation == "getStockList":
            result = getStockList(*args)
        elif operation == "getStockInfo":
            result = getStockInfo(*args)
        elif operation == "getMinutelyPrice":
            result = getMinutelyPrice(*args)
        elif operation == "getDailyPrice":
            result = getDailyPrice(*args)
        elif operation == "getWeeklyPrice":
            result = getWeeklyPrice(*args)
        elif operation == "checkTradeStatus":
            result = checkTradeStatus(*args)
        elif operation == "exit":
            with HiddenPrints():
                BaoStock.logout()
            break
        json = Json.dumps(result, ensure_ascii=False)
        System.stdout.write(json)
        System.stdout.write("\n")
        System.stdout.flush()
