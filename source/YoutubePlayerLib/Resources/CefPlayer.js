// 2. This code loads the IFrame Player API code asynchronously.
  var tag = document.createElement('script');

  tag.src = "https://www.youtube.com/iframe_api";
  var firstScriptTag = document.getElementsByTagName('script')[0];
  firstScriptTag.parentNode.insertBefore(tag, firstScriptTag);

  // 3. This function creates an <iframe> (and YouTube player)
  //    after the API code downloads.
  var player;
  var autoPlay = false;
  var quality = "hd720";
  var startUpId = 'XIMLoLxmTDw';
  
  function onYouTubeIframeAPIReady() {
	player = new YT.Player('player', {
	  height: '390',
	  width: '640',
	  videoId: startUpId,
	  suggestedQuality: "hd720",
	  events: {
		'onReady': onPlayerReady,
		'onStateChange': onPlayerStateChange,
		'onPlaybackQualityChange' : onPlayerPlaybackQualityChange
	  }
	});
  }

  function onPlayerReady(event) {
	bound.playerLoaded();
  }

  function onPlayerStateChange(event) {

    bound.playingChanged(event.data);
  }
  
  function onPlayerPlaybackQualityChange(event){
	  bound.qualityChanged();
  }
  
function setPlayerState(state) {
	if(state == "stop")
		player.stopVideo();
	else if (state == "start")
		player.playVideo();
	else if(state == "pause")
		player.pauseVideo();
	
}
  
function setVolume(volume){
	player.setVolume(volume);
}

function setVideoId(videoId){
	if(autoPlay){
		player.loadVideoById(videoId,0,quality);
	}
	else{
		player.cueVideoById(videoId,0,quality);
	}
}

function setQuality(quality){
	player.setPlaybackQuality(quality);
}