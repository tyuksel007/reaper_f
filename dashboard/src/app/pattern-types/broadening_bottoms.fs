export interface BroadeningBottom {
    symbol: string;
    time: string; 
    close: number;
    open: number;
    high: number;
    low: number;
    interval: string;
    volume: number;

    pivotLow: number;
    pivotHigh: number;
    intercept_high: number;
    intercept_low: number;
    slope_high: number;
    slope_low: number;
    rSquared_high: number;
    rSquared_low: number;
}