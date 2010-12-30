ReportGenerator README
======================
ReportGenerator is a small console .NET application that can be used to get software, hardware and network information from a windows workstation.  The application can gather specific information (see command line arguments below).  WMI and the Registry is used to get the requested information from the workstation. 

Information Gathered
--------------------
Depending on the parameter passed to the application, different type of information is gathered from the computer. When the 'c' parameter is passed, the application gathers basic information about the computer (i.e. service pack, type of os, etc.).  When the 'h' paramter is passed, you will be able to gather hardware information about the computer (i.e. cpu, ram, video card, sound card, etc.).  When the 'n' parameter is passed, you will be able to get network information about the computer (i.e. ip address, hostname, etc.).  When the 's' parameter is passed, you will get the software that is currently installed on the machine.  Software information is retrieved from the registry. If you want to generate all the infomration at once, use the 'a' parameter. 

Command Line Arguments
----------------------
You can pass the following parameters to gather specific data about a computer (Note: no dash infront of the parameter is required):

ReportGenerator a  - Grabs all the computer information 
ReportGenerator c  - Grabs the computer information 
ReportGenerator h  - Grabs the computer hardware information 
ReportGenerator n  - Grabs the computer network information 
ReportGenerator s  - Grabs the computer sofware information 

Additional Notes
----------------
At this time, the application can only be executed on a local machine you cannot run this application on the network at this time.  The application was originally written on top of .NET 2.0 so this is a minimum requirement on the client workstation where it's executed on.  The application uses a mysql database to store the information gathered.  You will need to download the MySql .NET Connetor at http://dev.mysql.com/downloads/connector/net/.  I have also included the schema for the tables where the data that is genereated is stored.  

