from io import TextIOWrapper
from typing import Any, Callable, Literal
from zlib import error as Error
from pandas.core.frame import DataFrame
from pathlib import Path
from datetime import datetime as DateTime, time as Time, timedelta as TimeSpan
from time import ctime
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


default: Callable[[str], str] = lambda x: x
fromPrice: Callable[[str], int] = lambda x: int(float(x) * 10000)
fromRate: Callable[[str], int] = lambda x: int(float(x) * 1000000)
fromPercent: Callable[[str], int] = lambda x: int(float(x) * 10000)
toInt: Callable[[str], int] = lambda x: int(x)
toFloat: Callable[[
    str], float] = lambda x: None if x == None or x == "" else float(x)
toFloored: Callable[[
    str], int] = lambda x: None if x == None or x == "" else int(x.split(".")[0])
toTime: Callable[[
    str], str] = lambda time: f"{time[0:4]}-{time[4:6]}-{time[6:8]}T{time[8:10]}:{time[10:12]}:{time[12:14]}.{time[14:]}Z"
toRehabilitation: Callable[[
    str], str] = lambda x: "1" if x == "post" else "2" if x == "pre" else "3"

priceMapBase: dict[str, tuple[str, Callable[[str], Any]]] = {
    "open": ("opening", fromPrice),
    "close": ("closing", fromPrice),
    "high": ("highest", fromPrice),
    "low": ("lowest", fromPrice),
    "volume": ("volume", toInt),
    "amount": ("turnover", fromPrice),
    "adjustflag": ("rehabilitation", toRehabilitation)
}
getStockInfoMap: dict[str, tuple[str, Callable[[str], Any]]] = {
    "code": ("id", default),
    "code_name": ("name", default),
    "ipoDate": ("listedDate", default),
    "outDate": ("delistedDate", default),
    "type": ("type", lambda x: "stock" if x == "1" else "index" if x == "2" else "other"),
    "industry": ("industry", default),
    "industryClassification": ("classification", default)
}
getStockListMap: dict[str, tuple[str, Callable[[str], Any]]] = {
    "code": ("id", default),
    "code_name": ("name", default)
}
getMinutelyPriceMap: dict[str, tuple[str, Callable[[str], Any]]] = priceMapBase | {
    "time": ("time", toTime)
}
getDailyPriceMap: dict[str, tuple[str, Callable[[str], Any]]] = priceMapBase | {
    "date": ("date", default),
    "preclose": ("preClosing", fromPrice),
    "turn": ("turnoverRate", fromRate),
    "peTTM": ("per", fromPercent),
    "pbMRQ": ("pbr", fromPercent),
    "psTTM": ("psr", fromPercent),
    "pcfNcfTTM": ("pcfr", fromPercent),
    "tradestatus": ("stopped", lambda x: x == "0"),
    "isST": ("specialTreatment", lambda x: x == "1")
}
getWeeklyPriceMap: dict[str, tuple[str, Callable[[str], Any]]] = priceMapBase | {
    "date": ("date", default),
    "turn": ("turnoverRate", fromRate),
}

statementMapBase: dict[str, tuple[str, Callable[[str], Any]]] = {
    "code": ("id", default),
    "pubDate": ("publishDate", default),
    "statDate": ("statDate", default),
}
getProfitabilityMap: dict[str, tuple[str, Callable[[str], Any]]] = statementMapBase | {
    "roeAvg": ("roe", toFloat),
    "npMargin": ("npm", toFloat),
    "gpMargin": ("gpm", toFloat),
    "netProfit": ("np", toFloored),
    "epsTTM": ("eps", toFloat),
    "MBRevenue": ("mbr", toFloored),
    "totalShare": ("ts", toFloored),
    "liqaShare": ("cs", toFloored)
}
getOperationalCapabilityMap: dict[str, tuple[str, Callable[[str], Any]]] = statementMapBase | {
    "NRTurnRatio": ("rtr", toFloat),
    "NRTurnDays": ("rtd", toFloat),
    "INVTurnRatio": ("itr", toFloat),
    "INVTurnDays": ("itd", toFloat),
    "CATurnRatio": ("catr", toFloat),
    "AssetTurnRatio": ("tatr", toFloat)
}
getGrowthAbilityMap: dict[str, tuple[str, Callable[[str], Any]]] = statementMapBase | {
    "YOYEquity": ("nagr", toFloat),
    "YOYAsset": ("tagr", toFloat),
    "YOYNI": ("npgr", toFloat),
    "YOYEPSBasic": ("bepsgr", toFloat),
    "YOYPNI": ("npasgr", toFloat)
}
getSolvencyMap: dict[str, tuple[str, Callable[[str], Any]]] = statementMapBase | {
    "currentRatio": ("cr", toFloat),
    "quickRatio": ("qr", toFloat),
    "cashRatio": ("car", toFloat),
    "YOYLiability": ("tlgr", toFloat),
    "liabilityToAsset": ("dar", toFloat),
    "assetToEquity": ("em", toFloat)
}
getCashFlowMap: dict[str, tuple[str, Callable[[str], Any]]] = statementMapBase | {
    "CAToAsset": ("catar", toFloat),
    "NCAToAsset": ("fatar", toFloat),
    "tangibleAssetToAsset": ("tatar", toFloat),
    "ebitToInterest": ("ipm", toFloat),
    "CFOToOR": ("oncforr", toFloat),
    "CFOToNP": ("oncfnpr", toFloat),
    "CFOToGr": ("oncfgrr", toFloat)
}
getPerformanceReportMap: dict[str, tuple[str, Callable[[str], Any]]] = {
    "code": ("id", default),
    "performanceExpPubDate": ("publishDate", default),
    "performanceExpStatDate": ("statDate", default),
    "performanceExpUpdateDate": ("updateDate", default),
    "performanceExpressTotalAsset": ("ta", toFloored),
    "performanceExpressNetAsset": ("na", toFloored),
    "performanceExpressEPSChgPct": ("epsgr", toFloat),
    "performanceExpressROEWa": ("roew", toFloat),
    "performanceExpressEPSDiluted": ("epsd", toFloat),
    "performanceExpressGRYOY": ("grgr", toFloat),
    "performanceExpressOPYOY": ("opgr", toFloat)
}
getPerformanceForecastMap: dict[str, tuple[str, Callable[[str], Any]]] = {
    "code": ("id", default),
    "profitForcastExpPubDate": ("publishDate", default),
    "profitForcastExpStatDate": ("statDate", default),
    "profitForcastType": ("type", default),
    "profitForcastAbstract": ("abstract", default),
    "profitForcastChgPctUp": ("npasgrUpperLimit", toFloat),
    "profitForcastChgPctDwn": ("npasgrLowerLimit", toFloat)
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
        result.append({getPerformanceForecastMap[key][0]: getPerformanceForecastMap[key][1](
            row[1][key]) for key in data.keys() if key in getPerformanceForecastMap})
    return result


startTime = Time(9, 30)
endTime = Time(15)


def checkTradeStatus(date: str = None):
    if date == None and (DateTime.now().time() < startTime or DateTime.now().time() > endTime):
        return "false"
    if date == None:
        date = str(DateTime.now().date())
    return "true" if BaoStock.query_trade_dates(date, date).get_row_data()[1] == "1" else "false"


def login():
    with HiddenPrints():
        login = BaoStock.login()
    if login.error_code != "0":
        raise Error("Connection failed")


if __name__ == "__main__":
    login()
    loginTime = DateTime.today()
    log = open(str(Path(__file__).parent.absolute()/"Log" /
               (DateTime.today().strftime("%Y-%m-%d-%H-%M-%S")+".log")), "w", encoding="utf8")
    System.stdout = TextIOWrapper(System.stdout.buffer, encoding='utf8')
    System.stdin = TextIOWrapper(System.stdin.buffer, encoding='utf8')
    for line in System.stdin:
        if (DateTime.today() - loginTime).total_seconds() > 3600:
            login()
            loginTime = DateTime.today()
        args = line.strip().split()
        operation = args[0]
        args = args[1:]
        result: str = ""
        validCommand: bool = True
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
        elif operation == "getPerformanceForecast":
            result = getPerformanceForecast(*args)
        elif operation == "exit":
            with HiddenPrints():
                BaoStock.logout()
            break
        else:
            validCommand = False
        json = Json.dumps(result, ensure_ascii=False)
        if validCommand:
            log.write(
                "\n" + DateTime.today().strftime("%Y-%m-%dT%H:%M:%S") + ": " + line + json + "\n")
        System.stdout.write(json)
        System.stdout.write("\n")
        System.stdout.flush()
