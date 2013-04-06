jQuery(document).ready(function(){
				jQuery('ul.tabs').each(function(){

					var $active, $content, $links = jQuery(this).find('a');

					$active = jQuery($links.filter('[href="'+location.hash+'"]')[0] || $links[0]);
					$active.addClass('active');
					$content = jQuery($active.attr('href'));

					$links.not($active).each(function () {
						jQuery(jQuery(this).attr('href')).hide();
					});

					jQuery(this).on('click', 'a', function(e){
						$active.removeClass('active');
						$content.hide();

						$active = jQuery(this);
						$content = jQuery(jQuery(this).attr('href'));

						$active.addClass('active');
						$content.show();

						e.preventDefault();
					});
				});
			});