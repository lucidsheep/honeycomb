#!/usr/bin/env node

import fs from 'fs';
import http from 'http';
import https from 'https';
import express from 'express';
import beehive from './listener/beehive.js'
import expressStaticGzip from 'express-static-gzip';
import { fileURLToPath } from 'url';
import expressMd from 'express-md';
import { join } from 'path';
import serveIndex from 'serve-index';

const __dirname = fileURLToPath(new URL('.', import.meta.url));
let overlayBuffer = fs.readFileSync(join(__dirname, "etc", "jsonOverlay.json"));
let overlayJson = overlayBuffer.toString();

const app = express();
const httpApp = express();

const privateKey = fs.readFileSync('/etc/letsencrypt/live/kq.style/privkey.pem', 'utf8');
const certificate = fs.readFileSync('/etc/letsencrypt/live/kq.style/cert.pem', 'utf8');
const ca = fs.readFileSync('/etc/letsencrypt/live/kq.style/fullchain.pem', 'utf8');

const credentials = {
	key: privateKey,
	cert: certificate,
	ca: ca
};

var mdRouter = expressMd({

    // serve markdown files from root
    dir: __dirname + '/md',
  
    // serve requests from root of the site
    url: '/',
  
    // variables to replace {{{ varName }}} in markdown files
    vars: {
    }
  });

// Starting both http & https servers
const httpServer = http.createServer(httpApp);
const httpsServer = https.createServer(credentials, app);

httpServer.listen(80, () => {
	console.log('HTTP Server running on port 80');
});

httpsServer.listen(443, () => {
	console.log('HTTPS Server running on port 443');
});

httpApp.get("*", function(req, res, next) {
    res.redirect("https://" + req.headers.host + req.path);
});

app.get('/overlay/:scene/:cabinet/obs_scene', function(req, res)
{
    let jsonString = overlayJson.replace("<scene>", req.params.scene).replace("<cab>", req.params.cabinet);
    res.attachment("KQIS_Scene.json");
    res.type("json");
    res.send(jsonString);
});

app.use('/overlay/:scene/:cabinet', expressStaticGzip('overlay', {
    enableBrotli: true,
    customCompressions: [{
        encodingName: 'br',
        fileExtension: 'unityweb'
    }],
    orderPreference: ['br']
}));

app.use('/bigoverlay/:scene/:cabinet', expressStaticGzip('bigoverlay', {
    enableBrotli: true,
    customCompressions: [{
        encodingName: 'br',
        fileExtension: 'unityweb'
    }],
    orderPreference: ['br']
}));

app.use('/webcamoverlay/:scene/:cabinet', expressStaticGzip('webcamoverlay', {
    enableBrotli: true,
    customCompressions: [{
        encodingName: 'br',
        fileExtension: 'unityweb'
    }],
    orderPreference: ['br']
}));

app.use('/roses/:scene/:cabinet', expressStaticGzip('roses', {
    enableBrotli: true,
    customCompressions: [{
        encodingName: 'br',
        fileExtension: 'unityweb'
    }],
    orderPreference: ['br']
}));

app.use('/debug/:scene/:cabinet', expressStaticGzip('debug', {
    enableBrotli: true,
    customCompressions: [{
        encodingName: 'br',
        fileExtension: 'unityweb'
    }],
    orderPreference: ['br']
}));

app.get('/.well-known/acme-challenge/05hpghoq0TxRPHoxhaOUBCsrHnbvMAZoa_HnCtAO-vE', (req, res) => {
  res.send('05hpghoq0TxRPHoxhaOUBCsrHnbvMAZoa_HnCtAO-vE.gv0-xgDqjDTXwuq_gxlXUlFgDs57YYSpWgzCLgcGd7Q')
})

app.get('/.well-known/acme-challenge/buo4BftydhRCQieAGFJZFz5e7aAcd6PHwN0lGB4C2Cs', (req, res) => {
  res.send('buo4BftydhRCQieAGFJZFz5e7aAcd6PHwN0lGB4C2Cs.gv0-xgDqjDTXwuq_gxlXUlFgDs57YYSpWgzCLgcGd7Q')
})

app.use('/', express.static(__dirname, {maxAge : 0}));

//listener(httpsServer);
beehive(httpsServer);
  // add as express middleware
  app.use(mdRouter);
