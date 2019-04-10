CREATE DATABASE `dsp` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */;

CREATE TABLE `chatlog` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `broadcastid` int(11) NOT NULL,
  `message` varchar(2000) NOT NULL,
  `date` datetime NOT NULL,
  `username` varchar(255) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `id_UNIQUE` (`id`),
  KEY `idx_chatlog_date` (`date`)
) ENGINE=InnoDB AUTO_INCREMENT=3802225 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

/* For sending huge queries to the sql server */
set GLOBAL max_allowed_packet = 1024*1024*1024;
set GLOBAL sql_mode='';