ANDROID TEST TOOL  
====

A Python tool to send different commands to multiple USB-connected Android phones.

How to use:  
----
* *Prerequisites*:  
  
All the phones plugged in must have [USB Debugging](https://developer.android.com/studio/debug/dev-options#enable) enabled, and no password (for the data and speed tests).  
  
* *simInfos.csv*: 
  
Enter the informations (phone number and IMSI) of all the phones connected via USB, as well as the index on each line.
**The phones will be referred via their index for the tests**.

* *testsToPerform.csv*:  
  
Enter the tests you want to perform, according to the syntax explained in [Available commands](#Available-commands)  

You can then run the Python script called `Test_Tool.py`
  
Available commands:  
----
(*note*: every command is case insensitive)  

* **MTC**  
  
Performs MTC between phones A and B. A calls B, B answers and A hangs up after detecting B has answered + the time desired by the user.
1 sec delay between consecutive calls and before picking up. A and B must be plugged in and 
written in simInfos.csv.  
  
Global syntax:  
&nbsp;&nbsp;&nbsp;&nbsp;MTC;Number of calls;Duration (sec.);FromPhoneA;ToPhoneB   
  
Example syntax of a line in testsToPerform.csv:  
&nbsp;&nbsp;&nbsp;&nbsp;`mtc;3;10;1;2`  
&nbsp;&nbsp;&nbsp;&nbsp;In this example, phone 1 will call phone 2 for 10 seconds, 3 times.
(*note*: the 10 seconds countdown begins **once phone 2 picks up the call**).  

* **MOC**  
  
Performs MOC from phone A to B. Phone A calls B, and after the time decided by the user, phone A hangs up.
Phone A must be plugged in and in simInfos.csv, Phone B can either be the index of a plugged phone or the
phone number of an other phone, not necessarily plugged.  
  
Global syntax:  
&nbsp;&nbsp;&nbsp;&nbsp;MOC;Number of calls;Duration (sec.);FromPhoneA;ToPhoneB  
  
Example syntax of a line in testsToPerform.csv:  
&nbsp;&nbsp;&nbsp;&nbsp;`moc;4;15;2;1`  
&nbsp;&nbsp;&nbsp;&nbsp;In this example, phone 2 will call phone 1 for 15 seconds, 4 times.
(*note*: the 10 seconds countdown begins **when phone 1 starts dialing**).

*	**SMS**  
  
Used to send multiple SMS from phone A to phone B, with the specified text.
Phone A must be plugged in and in simInfos.csv, Phone B can either be the index of a plugged phone or the
phone number of an other phone, not necessarily plugged.  
  
Global syntax:  
&nbsp;&nbsp;&nbsp;&nbsp;SMS;Number of SMSs;Text;FromPhoneA;ToPhoneB  
  
Example syntax of a line in testsToPerform.csv:  
&nbsp;&nbsp;&nbsp;&nbsp;`sms;20;Example Text!;1;3`  
&nbsp;&nbsp;&nbsp;&nbsp;In this example, phone 1 will send `Example text!` to phone 3, 20 times.
(*note*: the double quote character (`"`) will be replaced by a single quote in the message).
  
* **Data**   
  
Performs a data test with specified phone, by opening the default browser and loading the given URL. 
The phone must **NOT** have a password for this to work.  
    
Global syntax:  
&nbsp;&nbsp;&nbsp;&nbsp;Data;Number of repetitions;URL;PhoneToUse
  
Example syntax of a line in testsToPerform.csv:  
&nbsp;&nbsp;&nbsp;&nbsp;`data;1;https://wwww.youtube.com;2`  
&nbsp;&nbsp;&nbsp;&nbsp;In this example, phone 2's browser will open, and go to `http://www.youtube.com` 1 time.
(*note*: the URL must be complete (`http://www.example.com`). If no URL is specified, like in `data;1;;1`, the default page to be opened is `google.com`).
  
* **Speedtest**  
  
Performs a speedtest by downloading a file from the specified phone and displaying the download speed in MBytes/sec. 
Will disable WiFi and use mobile data. The phone must **NOT** have a password for this to work.  
  
Global syntax:  
&nbsp;&nbsp;&nbsp;&nbsp;speedtest;PhoneToUse;FileSize  
  
Example syntax of a line in testsToPerform.csv:  
&nbsp;&nbsp;&nbsp;&nbsp;`speedtest;1;20`  
&nbsp;&nbsp;&nbsp;&nbsp;In this example, the speedtest will be operated from phone 1 (*note*: The file size **must** be one of 10, 20, 50 or 100), else the test will not be run).














