var static 	= require('node-static');
var http 	= require('http');
var file 	= new(static.Server)('../client');
var colors  = require('colors');
var winston = require('winston');



winston.add(winston.transports.File, { filename: 'console.log' });

var httpServer = http.createServer(function (request, response) {
	request.addListener('end', function () {
		file.serve(request, response);
	});
});

httpServer.listen(8080);



winston.info('Static file server running @ '+'10.2.4.26'.cyan+':'+'8080'.red);


var nowjs 		= require('now');
var everyone 	= nowjs.initialize(httpServer);


var buffer 	= require('buffer').Buffer;
var dgram 	= require('dgram');

sock = dgram.createSocket("udp4", function (msg, rinfo) {
  var points = msg.toString('ascii',0,rinfo.size).split(' ');
  var ball 	= {};
  var av 		= {};

  if(points.length == 3){
	  ball.p = rinfo.port;
	 	ball.x = parseFloat(points[0]);
	  ball.y = parseFloat(points[1]);
		ball.z = parseFloat(points[2]);

		console.log('X: ', ball.x);
 		console.log('Y: ', ball.y);
 		console.log('Z: ', ball.z);

		everyone.now.handleBall(ball);

 	} else {
 		av.p = rinfo.port;
 		av.x = parseFloat(points[0]);
 		console.log('handle av', av);

 		everyone.now.handleAv(av);
 	}

});

sock.bind(3890);
winston.info('UDP Port @ '+'10.2.4.26'.cyan+':'+'3890'.red);