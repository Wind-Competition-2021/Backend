from io import TextIOWrapper
from typing import Any, Callable, Literal
import typing
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


rawMapper: Callable[[str], str] = lambda x: x
priceMapper: Callable[[str], int] = lambda x: int(float(x)*10000)
rateMapper: Callable[[str], int] = lambda x: None if x == None or x == "" else int(
    float(x)*1000000)
intMapper: Callable[[str], int] = lambda x: int(x)
floorMapper: Callable[[
    str], int] = lambda x: None if x == None or x == "" else int(x.split(".")[0])
timeMapper: Callable[[
    str], str] = lambda time: f"{time[0:4]}-{time[4:6]}-{time[6:8]}T{time[8:10]}:{time[10:12]}:{time[12:14]}.{time[14:]}Z"
rehabilitationMapper: Callable[[
    str], str] = lambda x: "1" if x == "post" else "2" if x == "pre" else "3"

priceMapBase: dict[str, tuple[str, Callable[[str], Any]]] = {
    "open": ("opening", priceMapper),
    "close": ("closing", priceMapper),
    "high": ("highest", priceMapper),
    "low": ("lowest", priceMapper),
    "volume": ("volume", intMapper),
    "amount": ("turnover", priceMapper),
    "adjustflag": ("rehabilitation", rehabilitationMapper)
}
getStockInfoMap: dict[str, tuple[str, Callable[[str], Any]]] = {
    "code": ("id", rawMapper),
    "code_name": ("name", rawMapper),
    "ipoDate": ("listedDate", rawMapper),
    "outDate": ("delistedDate", rawMapper),
    "type": ("type", lambda x: "stock" if x == "1" else "index" if x == "2" else "other"),
    "industry": ("industry", rawMapper),
    "industryClassification": ("classification", rawMapper)
}
getStockListMap: dict[str, tuple[str, Callable[[str], Any]]] = {
    "code": ("id", rawMapper),
    "code_name": ("name", rawMapper)
}
getMinutelyPriceMap: dict[str, tuple[str, Callable[[str], Any]]] = priceMapBase | {
    "time": ("time", timeMapper)
}
getDailyPriceMap: dict[str, tuple[str, Callable[[str], Any]]] = priceMapBase | {
    "date": ("date", rawMapper),
    "preclose": ("preClosing", priceMapper),
    "turn": ("turnoverRate", rateMapper),
    "peTTM": ("per", rateMapper),
    "pbMRQ": ("pbr", rateMapper),
    "psTTM": ("psr", rateMapper),
    "pcfNcfTTM": ("pcfr", rateMapper),
    "tradestatus": ("stopped", lambda x: x == "0"),
    "isST": ("specialTreatment", lambda x: x == "1")
}
getWeeklyPriceMap: dict[str, tuple[str, Callable[[str], Any]]] = priceMapBase | {
    "date": ("date", rawMapper),
    "turn": ("turnoverRate", rateMapper),
}

statementMapBase: dict[str, tuple[str, Callable[[str], Any]]] = {
    "code": ("id", rawMapper),
    "pubDate": ("publishDate", rawMapper),
    "statDate": ("statDate", rawMapper),
}
getProfitabilityMap: dict[str, tuple[str, Callable[[str], Any]]] = statementMapBase | {
    "roeAvg": ("roe", rateMapper),
    "npMargin": ("npm", rateMapper),
    "gpMargin": ("gpm", rateMapper),
    "netProfit": ("np", floorMapper),
    "epsTTM": ("eps", rateMapper),
    "MBRevenue": ("mbr", floorMapper),
    "totalShare": ("ts", floorMapper),
    "liqaShare": ("cs", floorMapper)
}
getOperationalCapabilityMap: dict[str, tuple[str, Callable[[str], Any]]] = statementMapBase | {
    "NRTurnRatio": ("rtr", rateMapper),
    "NRTurnDays": ("rtd", rateMapper),
    "INVTurnRatio": ("itr", rateMapper),
    "INVTurnDays": ("itd", rateMapper),
    "CATurnRatio": ("catr", rateMapper),
    "AssetTurnRatio": ("tatr", rateMapper)
}
getGrowthAbilityMap: dict[str, tuple[str, Callable[[str], Any]]] = statementMapBase | {
    "YOYEquity": ("nagr", rateMapper),
    "YOYAsset": ("tagr", rateMapper),
    "YOYNI": ("npgr", rateMapper),
    "YOYEPSBasic": ("bepsgr", rateMapper),
    "YOYPNI": ("npasgr", rateMapper)
}
getSolvencyMap: dict[str, tuple[str, Callable[[str], Any]]] = statementMapBase | {
    "currentRatio": ("cr", rateMapper),
    "quickRatio": ("qr", rateMapper),
    "cashRatio": ("car", rateMapper),
    "YOYLiability": ("tlgr", rateMapper),
    "liabilityToAsset": ("dar", rateMapper),
    "assetToEquity": ("em", rateMapper)
}
getCashFlowMap: dict[str, tuple[str, Callable[[str], Any]]] = statementMapBase | {
    "CAToAsset": ("catar", rateMapper),
    "NCAToAsset": ("fatar", rateMapper),
    "tangibleAssetToAsset": ("tatar", rateMapper),
    "ebitToInterest": ("ipm", rateMapper),
    "CFOToOR": ("oncforr", rateMapper),
    "CFOToNP": ("oncfnpr", rateMapper),
    "CFOToGr": ("oncfgrr", rateMapper)
}
getPerformanceReportMap: dict[str, tuple[str, Callable[[str], Any]]] = {
    "code": ("id", rawMapper),
    "performanceExpPubDate": ("publishDate", rawMapper),
    "performanceExpStatDate": ("statDate", rawMapper),
    "performanceExpUpdateDate": ("updateDate", rawMapper),
    "performanceExpressTotalAsset": ("ta", floorMapper),
    "performanceExpressNetAsset": ("na", floorMapper),
    "performanceExpressEPSChgPct": ("epsgr", rateMapper),
    "performanceExpressROEWa": ("roew", rateMapper),
    "performanceExpressEPSDiluted": ("epsd", rateMapper),
    "performanceExpressGRYOY": ("grgr", rateMapper),
    "performanceExpressOPYOY": ("opgr", rateMapper)
}
getPerformanceForcastMap: dict[str, tuple[str, Callable[[str], Any]]] = {
    "code": ("id", rawMapper),
    "profitForcastExpPubDate": ("publishDate", rawMapper),
    "profitForcastExpStatDate": ("statDate", rawMapper),
    "profitForcastType": ("type", rawMapper),
    "profitForcastAbstract": ("abstract", rawMapper),
    "profitForcastChgPctUp": ("npasgrUpperLimit", rateMapper),
    "profitForcastChgPctDwn": ("npasgrLowerLimit", rateMapper)
}


def getStockInfo(id: str):
    data1 = BaoStock.query_stock_basic(id).get_data()
    data2 = BaoStock.query_stock_industry(id).get_data()
    return {getStockInfoMap[key][0]: getStockInfoMap[key][1](data1.at[0, key]) for key in data1.keys() if key in getStockInfoMap} \
        | {getStockInfoMap[key][0]: getStockInfoMap[key][1](data2.at[0, key]) for key in data2.keys() if key in getStockInfoMap}


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
            result.append({getStockListMap[key][0]: row[key]
                          for key in keys if key in getStockListMap})
        return result
    for row in data.iterrows():
        result.append({getStockListMap[key][0]: row[1][key]
                      for key in data.keys() if key in getStockListMap})
    return result


def getMinutelyPrice(id: str, begin: str = None, end: str = None, frequency: str = "60", rehabilitation: Literal["pre", "post", "none"] = "none"):
    result = []
    rehabilitation = "1" if rehabilitation == "post" else "2" if rehabilitation == "pre" else "3"
    data = BaoStock.query_history_k_data_plus(
        id, ",".join(getMinutelyPriceMap.keys()), begin, end, frequency, rehabilitation).get_data()
    for row in data.iterrows():
        dataRow = {getMinutelyPriceMap[key][0]: getMinutelyPriceMap[key][1](
            row[1][key]) for key in data.keys() if key in getMinutelyPriceMap}
        dataRow["id"] = id
        result.append(dataRow)
    return result


def getDailyPrice(id: str, begin: str = None, end: str = None, rehabilitation: Literal["pre", "post", "none"] = "none"):
    result = []
    rehabilitation = "1" if rehabilitation == "post" else "2" if rehabilitation == "pre" else "3"
    data = BaoStock.query_history_k_data_plus(
        id, ",".join(getDailyPriceMap.keys()), begin, end, "d", rehabilitation).get_data()
    for row in data.iterrows():
        dataRow = {getDailyPriceMap[key][0]: getDailyPriceMap[key][1](
            row[1][key]) for key in data.keys() if key in getDailyPriceMap}
        dataRow["id"] = id
        result.append(dataRow)
    return result


def getWeeklyPrice(id: str, begin: str = None, end: str = None, frequency: str = "week", rehabilitation: Literal["pre", "post", "none"] = "none"):
    result = []
    rehabilitation = "1" if rehabilitation == "post" else "2" if rehabilitation == "pre" else "3"
    data = BaoStock.query_history_k_data_plus(
        id, ",".join(getWeeklyPriceMap.keys()), begin, end, "w" if frequency == "week" else "m", rehabilitation).get_data()
    for row in data.iterrows():
        dataRow = {getWeeklyPriceMap[key][0]: getWeeklyPriceMap[key][1](
            row[1][key]) for key in data.keys() if key in getWeeklyPriceMap}
        dataRow["id"] = id
        result.append(dataRow)
    return result


def getProfitability(id: str, yea: str = None, quarter: str = None):
    data = BaoStock.query_profit_data(id, yea, quarter).get_data()
    return {getProfitabilityMap[key][0]: getProfitabilityMap[key][1](data.at[0, key]) for key in data.keys() if key in getProfitabilityMap}


def getOperationalCapability(id: str, yea: str = None, quarter: str = None):
    data = BaoStock.query_operation_data(id, yea, quarter).get_data()
    return {getOperationalCapabilityMap[key][0]: getOperationalCapabilityMap[key][1](data.at[0, key]) for key in data.keys() if key in getOperationalCapabilityMap}


def getGrowthAbility(id: str, yea: str = None, quarter: str = None):
    data = BaoStock.query_growth_data(id, yea, quarter).get_data()
    return {getGrowthAbilityMap[key][0]: getGrowthAbilityMap[key][1](data.at[0, key]) for key in data.keys() if key in getGrowthAbilityMap}


def getSolvency(id: str, yea: str = None, quarter: str = None):
    data = BaoStock.query_balance_data(id, yea, quarter).get_data()
    return {getSolvencyMap[key][0]: getSolvencyMap[key][1](data.at[0, key]) for key in data.keys() if key in getSolvencyMap}


def getCashFlow(id: str, yea: str = None, quarter: str = None):
    data = BaoStock.query_cash_flow_data(id, yea, quarter).get_data()
    return {getCashFlowMap[key][0]: getCashFlowMap[key][1](data.at[0, key]) for key in data.keys() if key in getCashFlowMap}


def getPerformanceReport(id: str, begin: str = None, end: str = None):
    data = BaoStock.query_performance_express_report(id, begin, end).get_data()
    result = []
    for row in data.iterrows():
        result.append({getPerformanceReportMap[key][0]: getPerformanceReportMap[key][1](
            row[1][key]) for key in data.keys() if key in getPerformanceReportMap})
    return result


def getPerformanceForecast(id: str, begin: str = None, end: str = None):
    data = BaoStock.query_forecast_report(id, begin, end).get_data()
    result = []
    for row in data.iterrows():
        result.append({getPerformanceForcastMap[key][0]: getPerformanceForcastMap[key][1](
            row[1][key]) for key in data.keys() if key in getPerformanceForcastMap})
    return result


startTime = Time(9, 30)
endTime = Time(15)


def checkTradeStatus(date: str = None):
    if date == None and (DateTime.now().time() < startTime or DateTime.now().time() > endTime):
        return "false"
    if date == None:
        date = str(DateTime.now().date())
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
        elif operation == "getProfitability":
            result = getProfitability(*args)
        elif operation == "getOperationalCapability":
            result = getOperationalCapability(*args)
        elif operation == "getGrowthAbility":
            result = getGrowthAbility(*args)
        elif operation == "getSolvency":
            result = getSolvency(*args)
        elif operation == "getCashFlow":
            result = getCashFlow(*args)
        elif operation == "getPerformanceReport":
            result = getPerformanceReport(*args)
        elif operation == "getPerformanceForcast":
            result = getPerformanceForecast(*args)
        elif operation == "exit":
            with HiddenPrints():
                BaoStock.logout()
            break
        json = Json.dumps(result, ensure_ascii=False)
        System.stdout.write(json)
        System.stdout.write("\n")
        System.stdout.flush()
