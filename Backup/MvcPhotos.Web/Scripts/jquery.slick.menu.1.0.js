/*

 * DC jQuery Slick - jQuery Slick
 * Copyright (c) 2011 Design Chemical
 *
 * Dual licensed under the MIT and GPL licenses:
 * 	http://www.opensource.org/licenses/mit-license.php
 * 	http://www.gnu.org/licenses/gpl.html
 *
 */

(function($){

	//define the new for the plugin ans how to call it
	$.fn.dcSlick = function(options) {

		//set default options
		var defaults = {
			classWrapper: 'dc-slick',
			classContent: 'dc-slick-content',
			idWrapper: 'dc-slick',
			location: 'left',
			align: 'top',
			offset: '100px',
			speed: 'slow',
			tabText: 'Click',
			classTab: 'tab',
			autoClose: true
		};

		//call in the default otions
		var options = $.extend(defaults, options);
		var $dcSlickObj = this;
		//act upon the element that is passed into the design
		return $dcSlickObj.each(function(options){

			var slickHtml = $dcSlickObj.html();
			var slickTab = '<div class="'+defaults.classTab+'"><span>'+defaults.tabText+'</span></div>';
			$dcSlickObj.hide();
			var slider = '<div id="'+defaults.idWrapper+'" class="'+defaults.classWrapper+'">'+slickTab+'<div class="'+defaults.classContent+'">'+slickHtml+'</div></div>';
			$('body').append(slider);
			var $slider = $('#'+defaults.idWrapper);
			var $tab = $('.'+defaults.classTab,$slider);
			$tab.css({position: 'absolute'});
			
			// Get container dimensions
		//	var width = $slider.width();
			var height = $slider.height();
			var outerW = $slider.outerWidth(true);
			var widthPx = outerW+'px';
			var outerH = $slider.outerHeight(true);
			var padH = outerH - height;
			var heightPx = outerH+'px';
			var bodyHeight = $(window).height();
			
			slickSetup($slider);
			
			if(defaults.autoClose == true){
				$('body').mouseup(function(e){
					if($slider.hasClass('active')){
						if(!$(e.target).parents('#'+defaults.idWrapper+' .'+defaults.classTab).length){
							slickClose();
						}
					}
				});
			}
			
			$tab.click(function(){
				if($slider.hasClass('active')){
					slickClose();
				} else {
					slickOpen();
				}
			});
	
			function slickOpen(){
			
				$('.'+defaults.classWrapper).css({zIndex: 10000});
				$slider.css({zIndex: 10001});
				if(defaults.location == 'bottom'){
					$slider.animate({marginBottom: "-=5px"}, "fast").animate({marginBottom: 0}, defaults.speed);
				}
				if(defaults.location == 'top'){
					$slider.animate({marginTop: "-=5px"}, "fast").animate({marginTop: 0}, defaults.speed);
				}
				if(defaults.location == 'left'){
					$slider.animate({marginLeft: "-=5px"}, "fast").animate({marginLeft: 0}, defaults.speed);
				}
				if(defaults.location == 'right'){
					$slider.animate({marginRight: "-=5px"}, "fast").animate({marginRight: 0}, defaults.speed);
				}
				$slider.addClass('active');
			}
			
			function slickClose(){
			
			$slider.css({zIndex: 10000});
			if($slider.hasClass('active')){
				if(defaults.location == 'bottom'){
					$slider.animate({"marginBottom": "-"+heightPx}, defaults.speed);
				}
				if(defaults.location == 'top'){
					$slider.animate({"marginTop": "-"+heightPx}, defaults.speed);
				}
				if(defaults.location == 'left'){
					$slider.animate({"marginLeft": "-"+widthPx}, defaults.speed);
				}
				if(defaults.location == 'right'){
					$slider.animate({"marginRight": "-"+widthPx}, defaults.speed);
				}
				$slider.removeClass('active');
			}
			}
			
			function slickSetup(obj){
				var $container = $('.'+defaults.classContent,obj);
				// Get slider border
				var bdrTop = $slider.css('border-top-width');
				var bdrRight = $slider.css('border-right-width');
				var bdrBottom = $slider.css('border-bottom-width');
				var bdrLeft = $slider.css('border-left-width');
				// Get tab dimension
				var tabWidth = $tab.outerWidth(true);
				var tabWidthPx = tabWidth+'px';
				var tabHeight = $tab.outerHeight(true);
				var tabHeightPx = tabHeight+'px';
				// Calc max container dimensions
				var containerH = $container.height();
				var containerPad = $container.outerHeight(true)-containerH;
				var maxHeight = bodyHeight - tabHeight;
				$(obj).addClass(defaults.location).addClass('align-'+defaults.align).css({position: 'fixed', zIndex: 10000});
				if(outerH > maxHeight){
					containerH = maxHeight - padH - containerPad;
					heightPx = maxHeight+'px';
				}
				$container.css({height: containerH+'px'});
				if(defaults.location == 'left'){
					$(obj).css({marginLeft: '-'+widthPx});
					$tab.css({marginRight: '-'+tabWidthPx});
					$(obj).css({top: defaults.offset});
				}
				
				if(defaults.location == 'right'){
					$(obj).css({marginRight: '-'+widthPx});
					$tab.css({marginLeft: '-'+tabWidthPx});
					$(obj).css({top: defaults.offset});
				}
				
				if(defaults.location == 'top'){
					$(obj).css({marginTop: '-'+heightPx});
					$tab.css({marginBottom: '-'+tabHeightPx});
					
					if(defaults.align == 'left'){
						$(obj).css({left: defaults.offset});
						$tab.css({left: 0});
					} else {
						$(obj).css({right: defaults.offset});
						$tab.css({right: 0});
					}
				}
				
				if(defaults.location == 'bottom'){
					$(obj).css({marginBottom: '-'+heightPx});
					$tab.css({marginTop: '-'+tabHeightPx});
					
					if(defaults.align == 'left'){
						$(obj).css({left: defaults.offset});
						$tab.css({left: 0});
					} else {
						$(obj).css({right: defaults.offset});
						$tab.css({right: 0});
					}
				}
			}

		});
	};
})(jQuery);