CREATE DATABASE /*!32312 IF NOT EXISTS*/ ComputerReport /*!40100 DEFAULT CHARACTER SET utf8 */;

USE "ComputerReport";

CREATE TABLE master_pcdetails (
  serial_number varchar(25) NOT NULL PRIMARY KEY,
  computer_name varchar(30) NOT NULL,
  registered_user varchar(20) NOT NULL,
  user_name varchar(100) NOT NULL,
  os_type varchar(50) NOT NULL,
  service_pack varchar(35) NOT NULL,
  ip_address varchar(55) NOT NULL,
  report_date varchar(25) NOT NULL
) ENGINE=InnoDB /*!40100 DEFAULT CHARSET=utf8 COLLATE=utf8_bin*/;

CREATE TABLE hardware_info (
  id int NOT NULL PRIMARY KEY AUTO_INCREMENT,
  serial_number varchar(25) REFERENCES master_pcdetails(serial_number),
  date_created varchar(25) NOT NULL,
  manufacturer varchar(30),
  manufacturer_model varchar(30),
  cpu varchar(50),
  ram int NOT NULL,
  hdd varchar(20),
  vdo varchar(100),
  snd varchar(100),
  nic varchar(100),
  optical_drive varchar(50),
  motherboard varchar(50)
) ENGINE=InnoDB /*!40100 DEFAULT CHARSET=utf8 COLLATE=utf8_bin*/;

CREATE TABLE network_info (
  id int NOT NULL PRIMARY KEY AUTO_INCREMENT,
  serial_number_id varchar(25) NOT NULL REFERENCES master_pcdetails(serial_number),
  date_created varchar(25) NOT NULL,
  ip_address varchar(16) NOT NULL,
  is_stolen BOOLEAN DEFAULT FALSE
) ENGINE=InnoDB;

CREATE TABLE software_info (
  id int(15) NOT NULL PRIMARY KEY AUTO_INCREMENT,
  date_created varchar(25) NOT NULL,
  serial_number varchar(25) NOT NULL REFERENCES master_pcdetails(serial_number), 
  app_name varchar(200) default NULL,
  is_update int(2) NOT NULL,
  version varchar(35) default NULL,
  publisher varchar(100) default NULL
) ENGINE=InnoDB /*!40100 DEFAULT CHARSET=utf8 COLLATE=utf8_bin*/;

