#!/usr/bin/env node

const { fork } = require('child_process');

//const listener = fork(__dirname + '/listener');

const server = fork(__dirname + '/webServer');

