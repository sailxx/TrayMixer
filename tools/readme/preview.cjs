const http = require('node:http');
const fs = require('node:fs');
const path = require('node:path');
const root = path.resolve(__dirname, '../..');
const server = http.createServer((req,res)=>{
 const pathname = decodeURIComponent(new URL(req.url,'http://localhost').pathname);
 const relative = pathname==='/'?'.sharrowkin/readme-preview/index.html':pathname.slice(1);
 const file = path.resolve(root,relative);
 if(!file.startsWith(root+path.sep) || !(relative.startsWith('assets/readme/') || relative==='.sharrowkin/readme-preview/index.html')) {res.writeHead(404);res.end('Not in preview');return;}
 fs.readFile(file,(err,data)=>{if(err){res.writeHead(404);res.end('Not found');return;}
 res.setHeader('Content-Type',file.endsWith('.svg')?'image/svg+xml':file.endsWith('.png')?'image/png':'text/html; charset=utf-8');res.end(data);});
});
server.listen(0,'127.0.0.1',()=>console.log('README preview: http://127.0.0.1:'+server.address().port));
