# Crypto Technical Analysis
Advanced Automated Cryptocurrency Trade Bot


## Introduction
A year ago, after realizing that the trading robots in the marketplace have many weaknesses, I decided to produce a trading robot called Chanel. The robots in the crypto market were often either inefficient, or inaccurate, or did not provide users with a good backtest. Many of them also needed programming knowledge to implement their strategies. Although some of them offer different features to the user for a lot of money, but they were still not usable with all their features. I myself bought two of the best ones before developing this project, but I could not use them due to their many problems. So I decided to develop my current product and make it available to everyone so they can use it if needed. This project went a little slow due to my busy schedule, and besides, I could not complete it. Many of its capabilities have already been completed and many capabilities and improvements can be made for this project. On this page I try to document everything. I will also gradually add all the versions of my local repository to the Github so that those who intend to work on it can fix any possible problems. It is worth mentioning that this project has been developed individually in overtime. I plan to create a product with unique features for the public by creating a group of programmers if funded.


## Product Features
* Structural capabilities
    * Can be used in the crypto market
    * Can be used for Binance exchange
    * Can be expanded for use on other exchanges
    * Ability to add various currency pairs to it
    * Has a centralized server and remote access by users (Client-Server)
    * Can be used by several users simultaneously
    * Ability to define user access levels
    * Has an admin user with the highest level of access
    * Ability to disable users
* Technical capabilities
    * Has a variety of time frames
    * Display data related to all time frames at once
    * Provide most important divergences online on all time frames
    * Has a list of indicators and oscillators for use by those who use them in trading
    * Provide a large number of support and resistance levels online on all time frames
    * Has a history of all instant information
* Backtest capabilities
    * Ability to implement the strategy with the product script itself
    * Ability to implement the strategy in C# and add it, while using the product
    * Ability to backtest the strategy at any time period
    * Has a large number of predefined rules
    * Possibility of backtest on price ticks
    * Ability to add complex alarms based on a strategy on different currencies
    * Ability to implement strategies and alarms for one currency based on other currencies
    * Has visual output for performed backtest
    * Ability to define money management on backtest
* User Interface capabilities
    * Ability to filter the displayed data
    * Has the ability to add and remove columns to existing grids
    * Automatic save Layout of all grids


## Future Works
* Redesign product
* Redesign features
* Redesign user interface
* Make the product interface user friendly with UI and UX
* Add web user interface
* Add automated trading
* Add price action functionality
* Add the ability to trade automatically based on strategies


## Goals
* Price action functionality
* Automated trading
* Pre-built crypto trading strategies
* Strategy market place
* Backtesting and paper trading
* All exchanges support


## Donations
I need financial help to continue the project. With financial support, I can work on the project and create a product that can compete with commercial products by creating a software team. In addition to troubleshooting, I can redesign the current project and add many required features to it. 
* Bitcoin Address: 1DHvP8j5YEq17DvBTrqVxMi9rGpNqzGFSj
* Ethereum Address: 0x39eCcdc933CDaA8253967D0e87834e50D0f0eCB4


## Product Setup
* Setup Microsoft SQL Server 2017 (Or any other SQL DBMS) and Create Database: Watch the video below to install the database and apply the required settings.

  [![IMAGE ALT TEXT HERE](https://img.youtube.com/vi/YYhKG-UAr-w/0.jpg)](https://www.youtube.com/watch?v=YYhKG-UAr-w)


* Setup Chanel: Watch the video below to configure the project and execute it. In order to update the candles, the settings related to connecting to Binance must be made, and then the two values BinanceKey and BinanceSecret must be included in the project code. After placing these two values, you must build the project in VisualStudio 2019. The project build process will take time due to the download of the packages used at that time. It should be noted that the first execution of the project will be very time consuming due to downloading the required information from Binance and building the database.

  [![IMAGE ALT TEXT HERE](https://img.youtube.com/vi/BVMx4zPYI7c/0.jpg)](https://www.youtube.com/watch?v=BVMx4zPYI7c)


## Product Use
The video below shows how to use this product briefly. In reviewing the different parts, an attempt has been made to film in such a way that by working a little with the software, all the features of this product can be used effectively.

  [![IMAGE ALT TEXT HERE](https://img.youtube.com/vi/96CyiSGLw20/0.jpg)](https://www.youtube.com/watch?v=96CyiSGLw20)


## Strategy and Alarm Samples
## Logical Operation:
* And: The evaluation result is correct only if all the conditions of its subset are correct.
```
#Sample
#BtcUsdt
#Long
#Condition
-And
{
	-BtcUsdt:0:Minute5:Increase (Field Name=Close, Previous Candle No=0)
	-BtcUsdt:0:Minute5:TouchField (Field Name 1=Close, Field Name 2=Ema20Value)
}
```
