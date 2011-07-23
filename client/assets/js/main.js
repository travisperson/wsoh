(function ($) {
	$.fn.court =  function (options) {
		
		var court = {};

		court.options = options;
		court.element = this.empty();
		court.images = {};
		court.images.background = new Image();
		court.images.avatar = new Image();
		court.av_data = {};

		court.ready = false;

		court.drawBackground = function() {
			court.context.clearRect(0, 0, court.context.canvas.width, court.context.canvas.height);
				// Set the style properties.
			court.context.fillStyle = '#ff6600';
			court.context.strokeStyle = '#fff';
			court.context.lineWidth = 4;

			var c_height = court.context.canvas.height;
			var c_width = court.context.canvas.width;

			court.context.beginPath();
						// Start from the top-left point.
			court.context.moveTo(0, c_height); // give the (x,y) coordinates

			// console.log(Math.ceil(c_width * .25),Math.ceil(.654 * c_height));
			court.context.lineTo(Math.ceil((c_width * .25)), Math.ceil(.654 * c_height));

			// console.log(Math.ceil(c_width * .75),Math.ceil(.654 * c_height));
			court.context.lineTo(Math.ceil((c_width * .75)), Math.ceil(.654 * c_height));

					//console.log(c_width, c_height);
			court.context.lineTo(c_width, c_height);

			// Done! Now fill the shape, and draw the stroke.
			// Note: your shape will not be visible until you call any of the two methods.
			court.context.fill();
			court.context.stroke();
			court.context.closePath();

			//rectangle

			court.context.fillStyle = '#5CBEFA';
			court.context.strokeStyle = '#ccc';
			court.context.lineWidth = 2;

			court.context.beginPath();
			// Start from the top-left point.
			court.context.moveTo(0, 0); // give the (x,y) coordinates
			court.context.lineTo(c_width, 0);
			court.context.lineTo(c_width, Math.ceil(c_height*.3649));
			court.context.lineTo(0, Math.ceil(c_height * .3649));

			court.context.fill();
			court.context.stroke();
			court.context.closePath();

			var image = new Image();
			image.src = court.context.canvas.toDataURL('image/png');

			return image;
		};


		court.init = function () {
			court.canvas  = document.createElement('canvas');
			court.canvas.height = court.options.height;
			court.canvas.width  = court.options.width;
			$(court.canvas).attr('id','court-canvas');				

			$(court.element).append(court.canvas);

			court.context = court.canvas.getContext('2d');

			court.images.background 	= court.drawBackground();
			court.images.avatar.src   = '/assets/images/avatar.png';

		};

		court.drawClear = function () {
			//console.log("drawClear()", (new Date().getTime()));
			
			/// Think of it has a giant erraser rectangle the size of the canvas
			/// that automatically clicks
			court.context.clearRect(0, 0, court.context.canvas.width, court.context.canvas.height);
		};

		court.drawCanvas = function () {
			court.drawClear();

			court.context.drawImage(court.images.background, 0, 0);
			

		};
		
		court.drawAv = function () {
			var cords = court.av_data;
			court.drawCanvas ();
			var scale = ((0.2+4)/8)
			var aspect = court.options.width/court.options.height;
			var x = court.options.width/2 + cords.x*scale*(court.options.width/8)*aspect // /*(cords.x + 2.0) **/ court.options.width - (court.options.width/(4-zmod));
			court.context.drawImage(court.images.avatar, x, parseInt(court.context.canvas.height)/2);
		}

		court.drawBall = function (cords) {

			console.debug("Cords: ", cords);
			
			// var x = (cords.x + 2.0) * (court.options.width/4.0);
			// var y = (cords.y + 2.0) * (court.options.height/4.0);
			// var z = cords.z * 10.0;

			
			var scale = ((cords.z+4)/8)
			var aspect = court.options.width/court.options.height;
			var x = court.options.width/2 + cords.x*scale*(court.options.width/8)*aspect // /*(cords.x + 2.0) **/ court.options.width - (court.options.width/(4-zmod));
			var y = court.options.height/2 + cords.y*scale*(court.options.width/8)
			var z = scale * 200;

			//console.debug("Zmod",zmod);
			//console.debug("W ",(court.options.width/(4-zmod)),"H",(court.options.height/(4-zmod)));
		

			console.debug(x,y,z);
			var strokeStyle = 'green';

			court.context.lineWidth = z;
			court.context.strokeStyle = strokeStyle;
			court.context.lineCap = 'round';
			court.context.beginPath();
				court.context.moveTo(x,y);
				court.context.lineTo(x,y);
			court.context.stroke();
		}
	

		console.debug('init');
		court.init();
		court.drawCanvas();

		return court;
					// function drawBG(c_width, c_height) {

					// var c = document.getElementById("court");
					// var context = c.getContext("2d");

					// var c2 = document.getElementById("court_avatar");
					// var c_avatar = c2.getContext("2d");

					// context.clearRect(0, 0, context.canvas.width, context.canvas.height);
					// // Set the style properties.
					// context.fillStyle = '#ff6600';
					// context.strokeStyle = '#fff';
					// context.lineWidth = 4;

					// context.beginPath();
					// // Start from the top-left point.
					// context.moveTo(0, c_height); // give the (x,y) coordinates

					// // console.log(Math.ceil(c_width * .25),Math.ceil(.654 * c_height));
					// context.lineTo(Math.ceil((c_width * .25)), Math.ceil(.654 * c_height));

					// // console.log(Math.ceil(c_width * .75),Math.ceil(.654 * c_height));
					// context.lineTo(Math.ceil((c_width * .75)), Math.ceil(.654 * c_height));

					// //console.log(c_width, c_height);
					// context.lineTo(c_width, c_height);

					// // Done! Now fill the shape, and draw the stroke.
					// // Note: your shape will not be visible until you call any of the two methods.
					// context.fill();
					// context.stroke();
					// context.closePath();

					// //rectangle

					// context.fillStyle = '#5CBEFA';
					// context.strokeStyle = '#ccc';
					// context.lineWidth = 2;

					// context.beginPath();
					// // Start from the top-left point.
					// context.moveTo(0, 0); // give the (x,y) coordinates
					// context.lineTo(c_width, 0);
					// context.lineTo(c_width, Math.ceil(c_height*.3649));
					// context.lineTo(0, Math.ceil(c_height * .3649));

					// context.fill();
					// context.stroke();
					// context.closePath();


					// var avatar = new Image();
					// avatar.src = "/assets/images/avatar.png";

					// c_avatar.drawImage(avatar, 0, 0);

					// var new_avatar = new Image();
					// new_avatar.src = c_avatar.canvas.toDataURL('image/png');

					// context.drawImage(new_avatar, 0, 0);

					// }
	}
})(jQuery);
