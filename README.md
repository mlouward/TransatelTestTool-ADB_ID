ANDROID TEST TOOL  
====

A WPF/Python/batch tool to send different commands to multiple USB-connected Android phones.  
WPF is used for the interface, Python for the testing architecture, and batch to send the commands to the phone via [ADB](https://developer.android.com/studio/command-line/adb).

How to use:  
----
* *Prerequisites*:  
  
All the phones plugged in must have [USB Debugging](https://developer.android.com/studio/debug/dev-options#enable) enabled, and no password (for the data and speed tests; swipe to unlock is also supported).  

Python 3.7+ must be installed and [added to Windows PATH](https://superuser.com/questions/143119/how-do-i-add-python-to-the-windows-path) (it can also be down at installation by checking "Add to Path")
  
* *simInfos.csv*: 
  
This file stores the IMSI and MSISDN of up to 99 SIM cards. Do not go over this number, else it will not work. The phones will be referred via their index in the testsToPerform.csv file, that is the fist number of each line in simInfos.

* *testsToPerform.csv*:  
  
This file stores all the current tests that will be run upon pressing "Launch tests". It is generated when pressing "Launch tests", based on the tests written in the interface.  

Interface  
----
  
* **Tests tab**  
  
This tab allows users to see which tests will be ran and in what order, as well as add tests with all their corresponding parameters.  
  
The parameters *Phone A*, *Delay after test (sec.)* and *Repetitions Number* are needed for every test.  
*Phone A* is the phone from where the test is ran.  
*Delay after test* is the time to wait after the test has been run, to run the next one.  
*Repetitions Number* allows to repeat a certain test.  
  
The interface provides syntax checking for all the fields (Phone numbers, integers only fields,...)
If no values are provided for Delay and Repetitions, the default values are 0 sec. and 1 repetition.  
  
Clicking "Add to List" will add the test to the list and write it to testsToPerform.csv.  
  
In the list of tests, you can select one or multiple tests and press the Del. key on the keyboard to delete tests.
You can also copy/paste them in the list. (However, deleting a **copied** test will delete its first occurence in the list. (To fix))  
  
You can save the current list of tests to a csv file by clicking on "Save Test File".
It will create a copy of testsToPerform.csv and save it in a separate folder (Saved Test Files).  
You can also load these test files by clicking "Load Test File". If you select multiple saved files, it will load all the files in the same order as in File Explorer.  
  
Finally, clicking on "Launch tests" will run the Python script called `Test_Tool.py`. This script will read testsToPerform.csv and execute all the tests in order.  
  
* **Phones tab**  
  
This tab allows users to see all the SIM cards entered in simInfos.csv, as well as informations on the phones *if they are plugged in* (Android version, phone model, root status).  
  
It also allows adding phones to this list as long as there are less than 99 phones.  
For rooted phones, you can also enable airplane mode for a certain duration from there (default is 3 seconds).  
  
If you select one or multiple phones in the list, you can press Ctrl+C to copy **the text** of the phone infos in Windows' clipboard.  
  
* **APN tab**  

This tab only works for **rooted phones**.  
  
Selecting a phone in the list will query for the available APNs in this phone.  
You can also add or delete APNs manually, and change the default APN.  
  
  
Available tests:  
----
(*note*: every command is case insensitive)  

* **MTC**  
  
Performs MTC between phones A and B. A calls B, B answers and A hangs up after detecting B has answered + the time entered by the user.
1 sec delay between consecutive calls and before picking up. A and B must be plugged in **and written** in simInfos.csv.  
  
(*note*: the user-chosen countdown begins **once phone B picks up the call**).  

* **MOC**  
  
Performs MOC from phone A to B. Phone A calls B, and after the time entered by the user, phone A hangs up.
Phone A must be plugged in **and written** in simInfos.csv, Phone B can either be the index of a plugged **and written** in simInfos.csv phone or the
phone number of an other phone, not necessarily plugged.  
  
(*note*: the user-chosen countdown begins **when phone A starts dialing**).

*	**SMS**  
  
Used to send multiple SMS from phone A to phone B, with the specified text.
Phone A must be plugged in **and written** in simInfos.csv, Phone B can either be the index of a plugged **and written** in simInfos.csv phone or the
phone number of an other phone, not necessarily plugged.  
  
(*note*: the double quote character (`"`) will be replaced by a single quote in the message).
  
* **Data**   
  
Performs a data test with specified phone, by opening the default browser and loading the given URL. 
Will disable WiFi and use mobile data. The phone must **NOT** have a password for this to work.  
  
(*note*: the URL must be complete (`http://www.example.com`). If no URL is specified, the default page to be opened is `google.com`).
  
* **Speedtest**  
  
Performs a speedtest by downloading a file from the specified phone and displaying the download speed in MBytes/sec. 
Will disable WiFi and use mobile data. The phone must **NOT** have a password for this to work.  
  
(*note*: The file size **must** be one of 10, 20, 50 or 100, else the test will not be run).

* **Ping**  
  
Performs a ping test from the specified phone to the desired IP address or URL (e.g. 8.8.8.8 or www.example.com).
Will disable WiFi and use mobile data.  
  
(*note*: The URL must **not** begin with `http://` for this test.)

* **Airplane**  
  
This test **only works for rooted phones**. Enables airplane mode on the specified phone (rooted) for the duration entered by the user, 
and then disables it.  
  
* **Change APN**  
  
This test **only works for rooted phones**. Allows to change the default APN for the selected phone, amongst the list of available APNs for the selected **rooted** phone (see APN tab).  
  
  



