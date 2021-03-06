# GwinstekLCRTester
![Build](https://forthebadge.com/images/badges/built-with-love.svg)
![Language](https://forthebadge.com/images/badges/made-with-c-sharp.svg)
![powered](https://forthebadge.com/images/badges/powered-by-electricity.svg)

![CC BY-NC-SA 4.0](https://img.shields.io/badge/License-CC%20BY--NC--SA%204.0-lightgrey.svg)
[![Version](https://badge.fury.io/gh/tterb%2FHyde.svg)](https://badge.fury.io/gh/tterb%2FHyde)
[![No Maintenance Intended](http://unmaintained.tech/badge.svg)](http://unmaintained.tech/)


## Table of Contents
* [How it works](#How-it-works)
* [GUI specification](#GUI-specification)
* [Device compatibility](#Device-compatibility)
* [CSV file format](#CSV-file-format)
* [Comments and adnotations](#Comments-and-adnotations)
* [Technologies used](#Used-technologies)
* [settings.json](#settingsjson)
* [Licence](#Licence)
* [Authors](#Authors)



## How it works


This application's main usage is to automatise measuring various parameters of capacitors on [Gwinstek LCR 6300](#Device-compatibility). Such measured data is then stored in csv files in order to easily open and read it in stylesheet programs. GwinstekLCRTester supports two main measure methods: 

* **Test of several capacitors (default)** : For this method user must specify one or more frequencies of measure and the [number of cycles](#Comments-and-adnotations). After hitting "Do tests" button we are prompted with inputed parameters on which tests will be performed. After completion of measurements for one capacitor message box appears asking ask whether we want to plug in another device or not

* **Serial test**: This test is performed only on one capacitor. Here, we have an additional parameter to specify, [AVG](#Comments-and-adnotations). After completing measurements program will prompt us information about it




## GUI specification

![GUI](https://github.com/Avngarde/GwinstekLCRTester/blob/main/README_images/GUI.png)



Before we start our application we must ensure that we connected Gwinstek device properly to our computer. To do so, we must find an RS-232 cable along with USB adapter and plug it into computer. Our program checks if COM ports are established and then GUI is loaded

GUI is divided into two "panels":


* Left panel : Here we specify parameters for connecting to device via RS-232. Check docs of your appliance for specific settings 

* Right panel : Is used for setting different frequencies, SI multipliers for params, measurement types and [additional parameter D](##Comments-and-adnotations). We can specify also numbers of cycles whether we serial test or not, set AVG param and output folder for csv data files


## Device compatibility

- Tested on Windows 10 Pro and Windows 7 Professional
- Tested on Gwinstek LCR 6300 model
- Compatible with Gwinstek LCR-6300/6200/6100/6020/6002 models according to [official manual](https://www.gwinstek.com/en-global/products/downloadSeriesDownNew/10208/754)


## CSV file format

![Example CSV file](https://github.com/Avngarde/GwinstekLCRTester/blob/main/README_images/csv_format.png)

Files are automatically named "pomiary_{D}.csv" where D is datetime in format dd-MM-yyyy--HH:mm:ss  
Above you can see an example of output for serial test (on one capacitor)  
Output csv file consists of serveral columns binded to fetched data. From the left we have :  

* Device number : for serial tests this is always 1
* Cycle number : resets to 1 on next device
* AVG : if not set in GUI, in csv it is listed as "NIE"
* First main parameter with multiplier : depends on chosen measurement type and multplier
* Second main parameter : it is never an unit that have multplier, in case of msType DCR it is not listed
* Additional parameter D: when not set in GUI it is listed as "NIE"
* Frequency : displayed as it was listed in GUI
* Date of measurement : in format of dd-MM-yyyy HH:mm:ss

## Comments and adnotations

- Number of cycles : indicates how many times we wish to measure measure parameters on one frequency. For example if we had 4 frequencies and clicked 2 cycles we get 2 measurements for each written frequency resulting in total of 8 tests

- AVG parameter : is set on device to get an average survey from given measurements. For example AVG 20 tells us that every mensuration in csv files is average of 20 mensurations in machine

- Additional parameter D : For measurement types that don't include parameter D we can force it's mensuration

- Speed of speed of serial test : Please note that serial test heavily depends on given AVG number. If we set AVG to 120, single test is going to last 49 seconds roughly! Since this is a machine calculations application cannot speed it up





## Technologies Used

- C# version v.8.0
- WPF
- Newtonsoft.Json v13.0.1
- System.IO.Ports v5.0.1






## config.json

![defualt settings.json](https://github.com/Avngarde/GwinstekLCRTester/blob/main/README_images/settings.png)

This file is automatically created and updated by application and should not be changed manually. settings.json sets default values in inputs such as RS connection data, so that you do not have to enter the same data over and over again. Default settings set is visible on the screenshoot (by defualt path to csv folder is next to .exe build file). After closing program, config file is automatically updated


## Licence

This work is licensed under a
[Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License](http://creativecommons.org/licenses/by-nc-sa/4.0/).


## Authors

- Jan Napieralski  [R3VANEK](https://github.com/R3VANEK)
- Kamil Paczkowski  [Avngarde](https://github.com/Avngarde)
