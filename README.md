# GwinstekLCRTester


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


This application's main usage is to automatise measuring various parameters of capacitors on [Gwinstek LCR 6300][#Device-compatibility]. Such measured data is then stored in csv files in order to easily open and read it in stylesheet programs. GwinstekLCRTester supports two main measure methods: 

* **Test of several capacitors (default)** : For this method user must specify one or more frequencies of measure and the (number of cycles)[#Comments-and-adnotations]. After hitting "Do tests" button we are prompted with inputed parameters on which tests will be performed. After completion of measurements for one capacitor message box appears asking ask whether we want to plug in another device or not

* **Serial test**: This test is performed only on one capacitor. Here, we have an additional parameter to specify, [AVG](#Comments-and-adnotations). After completing measurements program will prompt us information about it




## GUI specification

Before we start our application we must ensure that we connected Gwinstek device properly to our computer. To do so, we must find an RS-232 cable along with USB adapter and plug it into computer. Our program checks if COM ports are established and then GUI is loaded

GUI is divided into two "panels":


* **Left panel** : Here we specify parameters for connecting to device via RS-232. Check docs of your appliance for specific settings 

* **Right panel** : Is used for setting different frequencies, SI multipliers for params, measurement types and [additional parameter D](##Comments-and-adnotations). We can specify also numbers of cycles whether we serial test or not, set AVG param and output folder for csv data files


## Device compatibility

- Tested on Windows 10 Pro and Windows 7 Professional
- Tested on Gwinstek LCR 6300 model
- Compatible with Gwinstek LCR-6300/6200/6100/6020/6002 models according to [official manual](https://www.gwinstek.com/en-global/products/downloadSeriesDownNew/10208/754)


## Comments and adnotations

- Number of cycles : indicates how many times we wish to measure measure parameters on one frequency. For example if we had 4 frequencies and clicked 2 cycles we get 2 measurements for each written frequency resulting in total of 8 tests

- AVG parameter : is set on device to get an average survey from given measurements. For example AVG 20 tells us that every mensuration in csv files is average of 20 mensurations in machine

- Additional parameter D : For measurement types that don't include parameter D we can force it's mensuration

- Speed of speed of serial test : Please note that serial test heavily depends on given AVG number. If we set AVG to 120, single test is going to last 43 seconds roughly! Since this is an machine calculations application cannot speed it up





## Technologies Used

- C# version 8.0
- WPF
- Newtonsoft.Json
- System.IO.Ports 






## config.json

![defualt settings.json](https://github.com/Avngarde/GwinstekLCRTester/README_img)

This file is automatically created and updated by application and should not be changed manually. settings.json sets default values in inputs such as RS connection data, so that you do not have to enter the same data over and over again. Default settings set is visible on the screenshoot (by defualt path to csv folder is next to .exe build file). After closing program, config file is updated


## Licence

All rights reserved


## Authors

- Jan Napieralski  [R3VANEK](https://github.com/R3VANEK)
- Kamil Paczkowski  [Avngarde](https://github.com/Avngarde)
