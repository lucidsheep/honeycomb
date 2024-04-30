#!/usr/bin/env node

import 'dotenv/config'
import { WebSocketServer, WebSocket } from 'ws'
import { format } from 'date-fns'
import mysql from 'mysql2'

function logWebsocket(socket, msg) {
  let message = format(new Date(), 'MM-dd HH:mm:ss') + '[' + socket.uid + ']: ' + msg.toString();
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

var wss; // = new WebSocket.Server({ server });

var db = mysql.createConnection({
  host     : 'localhost',
  user     : 'node',
  password : process.env.MYSQL_PWD,
  database : 'tournament'
});

db.connect();

export default async (expressServer) => {
  wss = new WebSocketServer({
    noServer: true,
    path: "/beehive"
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
      uid = uid + 1;
      logWebsocket(ws, 'New connection ');
      connectedOverlays.add({uid: ws.uid, ws: ws});
    
      ws.on('pong', heartbeat);
    
      ws.on('message', messageString => {
        var message = JSON.parse(messageString);
        if(!message)
        {
          logWebsocket(ws, "invalid message received");
          return;
        } 
        message.ws = ws;
        queuedMessages.push(message);
        
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
      });  
    });
  return wss;
};

const interval = setInterval(() => {
  if(!wss) return;
  wss.clients.forEach(ws => {
    if(!ws.isAlive)
      return ws.terminate();

    ws.isAlive = false;
    ws.ping(() => {});
  });
}, 30000);

const queuedMessages = [];
const connectedOverlays = new Set();
var uid = 0;

function processQueue() {

  if (queuedMessages.length === 0)
  {
    return;
  }

  const messageBatch = queuedMessages.splice(0, Math.min(queuedMessages.length, 100));

  for (const message of messageBatch) {
    const scene = message.scene;
    const type = message.type;
    const table = 'leaderboard';

    switch(type)
    {
      case "gameEnd":
      for(const player of message.players)
      {
        log(JSON.stringify(player));
        const hivemindID = player.id;

        db.query('SELECT * FROM ' + table + ' WHERE id = ' + hivemindID, function (error, results, fields) {
          if (error) throw error;
          if(results.length > 0)
          {
            //merge
            var curData = results[0];
            player.berries += curData.berries;
            player.berries_kicked += curData.berries_kicked;
            player.deaths += curData.deaths;
            player.kills_all += curData.kills_all;
            player.kills_military += curData.kills_military;
            player.kills_queen += curData.kills_queen;
            player.kills_queen_aswarrior += curData.kills_queen_aswarrior;
            player.warrior_deaths += curData.warrior_deaths;
            player.warrior_uptime += curData.warrior_uptime;
            player.snail += curData.snail;
            player.jason_points += curData.jason_points;
            player.snail_deaths += curData.snail_deaths;
          }

          player.warrior_ratio = player.kills_all == 0 || player.warrior_uptime == 0 ? 100.0 : (player.kills_all / player.warrior_uptime) * 60.0;     
          db.query(`REPLACE INTO ${table} VALUES (${player.id}, "${player.name}", ${player.kills_military}, ${player.kills_queen}, ${player.kills_queen_aswarrior}, ${player.berries}, ${player.snail}, ${player.berries_kicked}, ${player.deaths}, ${player.warrior_uptime}, ${player.kills_all}, ${player.warrior_ratio}, ${player.warrior_deaths}, ${player.snail_deaths}, ${player.jason_points}, "${scene}");`, function (err2, res2, fields2) {
            if (err2) throw err2;
          });   
        });

      }
      break;
      case "getLeaderboard":
        const leaderboardName = message.leaderboard;
        broadcastLeaderboard(leaderboardName, scene, message.ws);
        break;
    }
  }
}

function broadcastLeaderboard(leaderboardName, scene, ws)
{
  const order = leaderboardName == "warrior_ratio" ? "ASC" : "DESC";
  db.query(`SELECT id, scene, name, ${leaderboardName} FROM leaderboard WHERE scene = "${scene}" ORDER BY ${leaderboardName} ${order} LIMIT 15;`, function (error, results, fields) {
    if (error) throw error;
    const msg = JSON.stringify({leaderboardName: leaderboardName, players : results});
    ws.send(msg);
  });
}
setInterval(processQueue, 100);
