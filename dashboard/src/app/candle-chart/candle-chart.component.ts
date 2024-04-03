import { Component, OnInit } from '@angular/core';
import * as Plotly from 'plotly.js'
import { Candle } from './candle-types';


@Component({
  selector: 'app-candle-chart',
  templateUrl: './candle-chart.component.html',
  styleUrls: ['./candle-chart.component.scss']
})

export class CandleChartComponent implements OnInit{
    constructor() { }

    ngOnInit(): void {
      this.plotCandleChart();
    }

    selectedExchange: string = ''; // Holds the selected value from the dropdown

    // This method is triggered when the "Scan" button is clicked
    scan(): void {
      if (!this.selectedExchange) {
        alert('Please select an exchange first.');
        return;
      }
      console.log(`Scanning ${this.selectedExchange}...`);

    }

    plotCandleChart() {
      const candles: Candle[] = [
        // Populate 
      ];

      const trace: Plotly.Data = {
        x: candles.map((candle) => candle.time),
        open: candles.map((candle) => candle.open),
        high: candles.map((candle) => candle.high),
        low: candles.map((candle) => candle.low),
        close: candles.map((candle) => candle.close),
        type: 'candlestick',
        text: candles.map((candle) => candle.symbol), // Optional for symbo
      };


      const layout = {
        title: 'Candlestick Chart',
        xaxis: {
          title: 'Time',
        },
        yaxis: {
          title: 'Price',
        },
      };

    Plotly.newPlot('candle-chart', [trace], layout, {responsive: true})
    }
}
