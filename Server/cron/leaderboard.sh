#!/bin/bash

mysql -u root tournament<<EOFMYSQL
delete from leaderboard where scene = '$1';
EOFMYSQL
