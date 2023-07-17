# AnomalyDetectionWebService

Flight Inspection Web Service, ex2, Advanced Programming 2, biu

# Introduction
A service for computing correlation between features, and detect the anomalies according to the normal model that had been learnt.
The project includes indeed 2 parts,
The first part is http server which handle request about the anomaly detection algorithms.The server implements the approach of REST.
The second part is client-side(browser page) which uses the server and enable non-technical people to interact with the server via the browser.

# Folder structure links
(AnomalyDetectionWebService shortcut is ADWS)
 * ADWS/      Root folder
 * [ADWS/ADWS/](AnomalyDetectionWebService/)  Source files of the server side
* [ADWS/ADWS/Controllers/](AnomalyDetectionWebService/Controllers/)  Source files of controllers which handle http request from server side
* [ADWS/ADWS/Models/](AnomalyDetectionWebService/Models/)  Source files of all logical management , mathematical anomaly detection algorithm and list of current "normal model of correlation" management.
* [ADWS/ADWS/Models/Types/](AnomalyDetectionWebService/Models/Types/)  Classes that are mentioned to define types. 
Note that some types are within other cs source files.
* [ADWS/ADWS/Models/Utils](AnomalyDetectionWebService/Models/Utils)  Statics classes, including IO_Utils and MathUtil
* [ADWS/ADWS/NormalModelsDB/](AnomalyDetectionWebService/NormalModelsDB/)   Folder to store the trained data, the correlative feature according to normal flight
* [ADWS/ADWS/Properties/](AnomalyDetectionWebService/Properties/)  Contains launchSettings.json to set if it's developing / production environment and ip + port for the server.
* [ADWS/ADWS/wwwroot/](AnomalyDetectionWebService/wwwroot/)  The folder which contain the page the server sends to the client. It's static resource of the server, but it operates dynamicaly in the client browser 

# License
Before cloning the project or using files in [ADWS/ADWS/wwwroot/](AnomalyDetectionWebService/wwwroot/) be aware that files under MIT License/other license are in used, for educational purpose only. Please check the license before! Those files might be imported within other files(like default.html) even if you not notice. for example:
* wwwroot/MIT/chart.min.js (Chart.js, MIT license) , [website](https://www.chartjs.org/) , [license info](https://www.chartjs.org/docs/latest/#license)
* wwwroot/MIT/ag-grid-community.min.js (AG Grid, MIT license) , [website](https://www.ag-grid.com/) , [license info](https://www.ag-grid.com/eula/AG-Grid-Community-License.html)
* wwwroot/MIT/jquery-3.6.0.min.js (jQuery, MIT license) , [website](https://jquery.org/) , [license info](https://jquery.org/license/)

# Pre requirements
* For establishing the server: asp .net core 5.0
* (Developing is recommended in visual studio 2019 with/without 'swagger' tool, you may need to double click in "solution explorer" on the AnomalyDetectionWebService.sln)
* For the client page : normal updated browser which support javascript and html

# Youtube link:
https://youtu.be/4rz1RIgKdrk

# CSV files
The csv files that are used to wwwroot/default.html should be with first line of features names seperated by comma. Rest of lines should be normal real numbers(float, not NaN / infinty) seperated by comma, each line has same amount of fields(like table, columns are the features, cells in rows(except for the header) are the values).
The test/detect csv file that uses the normal model which was learned by the train file, the test file must contain at least all the features that was at the train file.
See examples in [ADWS/ADWS/wwwroot/train_test_csv](AnomalyDetectionWebService/wwwroot/train_test_csv).

# Further words
* The project isn't suitable for private / secure communication, both from technical issues, and not taking security as most important thing while developing it.
* AnomalyDetectoion class can support trivial algorithms, while all you need in order to add your algorithm is to implements ThresholdMethods and CheckerMethods correctly and add them to the dictionaries in the class.
* I recommend use swagger, read comments at AnomalyDetectionController.cs, and if need, see the Request/response object that are resolved via json parse to get idea what each uri can give ,what possible return http status can be, and what is the way to use the uri correctly
* For developers for this program - server side - be careful about asyncronic programming, match-type from client(for example check fields aren't null, corrent range of num etc), json resolver (works for known + public proprties when make json to string)
* See also the uri description + Uml classes diagram for server side [Uri_And_Uml.pdf](Uri_And_Uml.pdf) and diagram for client side [client_side_diagram.pdf](client_side_diagram.pdf).
* Note that even if the exception is caught in debugging it might seems that it doesn't. (It is disadvantage so you can't 
run reliable server from the debugger)
From (first "note" box) : https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/exception-handling-task-parallel-library
