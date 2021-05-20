import express = require("express");
import clone = require("lodash.clonedeep");

enum RangeType {
	Below = -1,
	Over = 1,
	Double = 2
}

function rand(base: number, range: number, type: RangeType = RangeType.Double) {
	const r = Math.abs(range) * base;
	const delta = (type == RangeType.Double ? (Math.random() - 0.5) : Math.random()) * type * r;
	return base + delta;
}

function rangeRand(left: number, right: number) {
	return left + (right - left) * Math.random();
}

const app = express();
const port = 8520;

interface Quote {
	name: string,
	opening: number,
	preClosing: number,
	closing: number,
	highest: number,
	lowest: number,
	volume: number,
	turnover: number
}

const quoteBase = new Map<string, Quote>()

app.get("/", (request, response) => {
	var ids = (request.query.list as string).split(',');
	const result: string[] = [];
	for (const id of ids) {
		if (!quoteBase.has(id)) {
			const opening = rand(100, 1);
			const closing = rand(opening, 0.1);
			const volume = rand(5000000, 1);
			const quote: Quote = {
				name: "name",
				opening: opening,
				preClosing: opening,
				closing: closing,
				highest: rangeRand(Math.max(opening, closing), opening * 1.1),
				lowest: rangeRand(Math.min(opening, closing), opening * 0.9),
				volume: volume,
				turnover: rand(volume * opening, 0.1)
			};
			quoteBase.set(id, quote);
		}
		const quote = clone(quoteBase.get(id)!);
		quote.closing = Math.max(quote.opening * 0.9, Math.min(quote.opening * 1.1, rand(quote.closing, 0.03)));
		quote.highest = Math.min(quote.opening * 1.1, Math.max(quote.highest, quote.closing));
		quote.lowest = Math.max(quote.opening * 0.9, Math.min(quote.lowest, quote.closing));
		quote.volume = rand(quote.volume, 0.01, RangeType.Over);
		quote.turnover = rand(quote.turnover, 0.01, RangeType.Over);
		let now: any = new Date();
		now.setHours(now.getHours() + 8);
		now = now.toISOString().split("T");
		const row = `${quote.name},${quote.opening.toFixed(4)},${quote.preClosing.toFixed(4)},${quote.closing.toFixed(4)},${quote.highest.toFixed(4)},${quote.lowest.toFixed(4)},0,0,${quote.volume.toFixed(0)},${quote.turnover.toFixed(4)},0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,${now[0]},${now[1].split(".")[0]},00,`;
		result.push(`var hq_str_${id}="${row}";`);
		quoteBase.get(id).highest = quote.highest;
		quoteBase.get(id).lowest = quote.lowest;
		quoteBase.get(id).volume = quote.volume;
		quoteBase.get(id).turnover = quote.turnover;
	}

	return response.send(result.join('\n'));
});

app.listen(port);