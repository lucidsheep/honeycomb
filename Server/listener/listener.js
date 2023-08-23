#!/usr/bin/env node

import { WebSocketServer, WebSocket } from 'ws'
import { v4 } from 'uuid'; // require('uuid');
import { writeFile, readFileSync, readdir } from 'fs'; // fs = require('fs');
import { join } from 'path';
import { fileURLToPath } from 'url';
import { format } from 'date-fns';

const __dirname = fileURLToPath(new URL('.', import.meta.url));

//don't set these both to true or weird things will happen
const generateFakeMessages = true;
const recordGames = false;

const scenecabToRecord = "kqpdxgroundkontrol";

var curRecording = "";
var recordStartTime = new Date();
var recordingInProgress = false;

var recordedGameFiles = [];
var recordedGameEvents = [];
var recordedGameEventIndex = 1;

function logWebsocket(socket, msg) {
  let message = format(new Date(), 'MM-dd HH:mm:ss') + '[' + socket.uid + ']: ' + msg.toString();
  if(socket.isCabinet)
    console.log('\x1b[36m%s\x1b[0m', message);
  else
    console.log(message);
}

function log(msg)
{
  let message = format(new Date(), 'MM-dd HH:mm:ss') + '[D]: ' + msg.toString();
  console.log(message);
}

function heartbeat() {
  this.isAlive = true;
}

var wss;

export default async (expressServer) => {
  wss = new WebSocketServer({
    noServer: true,
    path: "/listener",
  });

  expressServer.on("upgrade", (request, socket, head) => {
    wss.handleUpgrade(request, socket, head, (websocket) => {
      wss.emit("connection", websocket);
    });
  });

  wss.on(
    "connection", ws => {
      ws.isAlive = true;
      ws.uid = uid;
      ws.isCabinet = false;
      uid = uid + 1;
      logWebsocket(ws, 'New connection ');
    
      ws.on('pong', heartbeat);
    
      ws.on('message', messageString => {
        var message = JSON.parse(messageString);
        if(!message)
        {
          logWebsocket(ws, "invalid message received");
          return;
        }
        message.ws = ws;
        if(message.type == 'overlay')
        {
          logWebsocket(ws, "Overlay attached");
            connectedOverlays.add({uid: message.ws.uid, ws: message.ws, scenecab: message.scene + message.cab});
        }
        else if(generateFakeMessages)
        {
          if(!ws.isCabinet)
          {
            ws.isCabinet = true;
            logWebsocket(ws, "Cabinet confirmed!");
          }
          //just bounce the message back if we're not accepting new messages
          ws.send(JSON.stringify({eventID: message.eventID}));
        }
        else if (!queuedMessagesByID.has(message.eventID)) {
          //logWebsocket(ws, "message added eid:" + message.eventID);
          queuedMessages.push(message);
          queuedMessagesByID.add(message.eventID);
          if(!ws.isCabinet)
          {
            ws.isCabinet = true;
            logWebsocket(ws, "Cabinet confirmed!");
          }
        } else if(queuedMessagesByID.has(message.eventID))
        {
          //logWebsocket(ws, "message ignored eid:" + message.eventID);
        }
      });
    
      ws.on('close', () => {
        var found = false;
        for (const overlay of connectedOverlays) {  
            if(overlay.uid == ws.uid)
            {
              logWebsocket(ws, 'Overlay disconnected');
                connectedOverlays.delete(overlay);
                found = true;
            }
        }
        if(found == false)
        {
          logWebsocket(ws, 'Cabinet disconnected');
        }
      });  
    });
  return wss;
};

const interval = setInterval(() => {
  if(!wss) return;
  var numClients = 0;
  var numOverlays = 0;
  wss.clients.forEach(ws => {
    if(!ws.isAlive)
      return ws.terminate();

    ws.isAlive = false;
    ws.ping(() => {});
    if(ws.isCabinet)
      numClients++;
    else
      numOverlays++;
  });
  log("Cabinets: " + numClients + " | Overlays: " + numOverlays + " | Messages: " + queuedMessages.length);
}, 30000);

const queuedMessages = [];
const queuedMessagesByID = new Set();
const disconnects = [];
const gameInfoCache = {};
const gameInfoLastQuery = {};

const connectedOverlays = new Set();
var uid = 0;

let rTypes = ['berryDeposit', 'blessMaiden', 'playerKill', 'getOnSnail', 'getOffSnail', 'victory', 'spawn'];

function rTypeToVals(type)
{
  let rPlayer = Math.floor(Math.random()*10) + 1;
  let rTarget = rPlayer == 0 ? 1 : rPlayer - 1;
  let rTeam = rPlayer % 2 == 1 ? "Red" : "Blue";
  let rX = Math.floor(Math.random()*1920);
  let rY = Math.floor(Math.random()*1080);
  var rTargetType = "";
  switch(type)
  {
    case 'berryDeposit': return [rX, rY, rPlayer];
    case 'blessMaiden': return [rX, rY, rTeam];
    case 'playerKill': rTargetType = rTarget <= 2 ? "Queen" : Math.floor(Math.random() * 2) ? "Soldier" : "Worker";  return [rX, rY, rPlayer, rTarget, rTargetType];
    case 'getOnSnail': return [rX, rY, rPlayer];
    case 'getOffSnail': return [rX, rY, null, rPlayer];
    case 'victory': return [rTeam, 'military'];
    case 'spawn': return [1, 'False'];
    default: return [];
  }
}

var fakeGameID = 1;

function createRandomFakeMessage()
{
  let t = rTypes[Math.floor(Math.random()*rTypes.length)];
  let vals = rTypeToVals(t);
  let d = new Date();
  createFakeMessage(t, vals, d);
}

function createFakeMessage(t, vals, d)
{
  let fMessage = {
    gameID: fakeGameID,
    eventID: v4(),
    time: d,
    type: t,
    values: vals,
    cabinet: {
      sceneName: 'kqpdx',
      cabinetName: 'groundkontrol',
      token: 'asdf',
    },
  };
  queuedMessages.push(fMessage);
  if(t == 'victory') fakeGameID++;
}

function genFakeMessage()
{
  if(curRecording === "")
  {
    if(!recordedGameFiles)
    {
      //no recordings exist, create random message
      let nextTimeout = Math.floor(Math.random() * 4000) + 1000;
      createRandomFakeMessage();
      setTimeout(genFakeMessage, nextTimeout);
      return;
    } else
    {
      
      let randFile = recordedGameFiles[Math.floor(Math.random()*recordedGameFiles.length)];
      let buffer = readFileSync(join(__dirname, "games", randFile));
      curRecording = buffer.toString();
      recordedGameEvents = curRecording.split(';');
      recordedGameEventIndex = 0;
      recordStartTime = new Date();
      log("starting playback of " + randFile);
      
    }
  }
  if(recordedGameEvents && recordedGameEvents.length > 0)
  {
    let thisEventParams = recordedGameEvents[recordedGameEventIndex].split('|');
    log("msg:" + recordedGameEvents[recordedGameEventIndex]);
    log("len: " + thisEventParams.length);
    let timestamp = parseInt(thisEventParams[0]);
    let type = thisEventParams[1];
    let vars = thisEventParams[2].split(',');
    let date = new Date(recordStartTime + timestamp);
    createFakeMessage(type, vars, date);
    
    recordedGameEventIndex += 1;
    if(recordedGameEventIndex < recordedGameEvents.length - 1)
    {
      let nextEventTimestamp = parseInt(recordedGameEvents[recordedGameEventIndex].split('|')[0]);
      let dt = nextEventTimestamp - timestamp;
      setTimeout(genFakeMessage, dt < 1 ? 1 : dt);
    } else
    {
      //game over, reset held data and long timeout before starting new game
      curRecording = "";
      setTimeout(genFakeMessage, 20000);
    }
  }
}
function processQueue() {
    if (queuedMessages.length === 0)
    {
      return;
    }
    const gamesEnded = [];

  const messageBatch = queuedMessages.splice(0, Math.min(queuedMessages.length, 100));
  for (const message of messageBatch) {
    const messageTime = new Date(message.time);
    
    var msg = JSON.stringify({
    event_type: message.type,
    values: message.values,
  });
    let scenecab = message.cabinet.sceneName + message.cabinet.cabinetName;
    for (const overlay of connectedOverlays) {
        if(overlay.scenecab === scenecab)
        {
            overlay.ws.send(msg);
        }
    }
    if(recordGames && scenecab === scenecabToRecord)
    {
        if(!recordingInProgress && message.type === 'spawn' && message.values[0] == 1)
        {
          //spawn(1) is the first message in a new game
          recordingInProgress = true;
          curRecording = "";
          recordStartTime = new Date();
          log("new game recording started");
        }
        if(recordingInProgress)
        {
            let timestamp = messageTime - recordStartTime;
            var valString = "";
            for(let i = 0; i < message.values.length; i++)
            {
              valString += message.values[i];
              if(i + 1 < message.values.length)
                valString += ",";
            }
            curRecording += timestamp + "|" + message.type + "|" + valString + ";";
            if(message.type === "victory")
            {
              if(timestamp < (1000 * 60 * 5)) //don't record games over 5 minutes, they are probably drills or messing around, not a real game
              {
                let fileName = join(__dirname, "games", format(recordStartTime, 'yyyy-MM-dd HH:mm:ss') + ".txt");
                writeFile(fileName, curRecording, function (err) {
                  if (err) return log(err);
                  log("recording '" + fileName + "' saved.");
                });
            } else
            {
              log("discarding game for being too long");
            }
              recordingInProgress = false;
              curRecording = "";
            }
        }
    }
    //log(msg);
    message.processed = true;
  }

  for (const message of messageBatch) {
    if (message.processed) {
      if (message.ws && message.ws.readyState === WebSocket.OPEN) {
        message.ws.send(JSON.stringify({eventID: message.eventID}));
        //logWebsocket(message.ws, "message processed eid:" + message.eventID);
      } else if(!message.ws)
      {
        log("message has no websocket, eid:" + message.eventID);
      } else if(message.ws.readyState != WebSocket.OPEN)
      {
        logWebsocket(message.ws, "message sender socket not open, eid:" + message.eventID);
      }
    } else {
      queuedMessages.push(message);
    }
  }
}

function tryProcessQueue() {
    processQueue();
    /*
    processQueue().then(() => {
    }).catch(error => {
      log('Error processing queue', error);
    });
    */
}

function cleanup() {
  const lastMessageByGame = {};
  for (const message of queuedMessages) {
    lastMessageByGame[message.gameID] = Math.max(new Date(message.time), lastMessageByGame[message.gameID] || 0);
  }

  const staleGames = new Set();
  const threshold = new Date() - 30 * 60000;

  for (const [gameID, messageTime] of Object.entries(lastMessageByGame)) {
    if (messageTime < threshold) {
      staleGames.add(gameID);
      log(`Discarding stale game ${gameID}`);
    }
  }

  if (staleGames.size > 0) {
    const messages = queuedMessages.splice(0, queuedMessages.length);
    for (message of messages) {
      if (staleGames.has(message.gameID)) {
        if (message.ws && message.ws.readyState === WebSocket.OPEN) {
          message.ws.send(JSON.stringify({eventID: message.eventID}));
        }
      } else {
        queuedMessages.push(message);
      }
    }
  }

}

setInterval(cleanup, 30000);

setInterval(tryProcessQueue, 100);
if(generateFakeMessages)
{
  readdir(join(__dirname, "games"), function (err, files) {
    if(err) {
      log("failed to scan for recorded games");
      setTimeout(genFakeMessage, 100);
    } else
    {
      log("found recorded games");
      recordedGameFiles = files;
      setTimeout(genFakeMessage, 100);
    }
  });
  
}
//server.listen(8080);
