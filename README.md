Csharp-YoutubePlayerControl
===========================

Wrapper between "Youtube javascript Iframe api" and WPF. 

Contains a WPF usercontrol that shows a youtube video with support for embedded events and functions. The control support almost all the events and functions the [javascript embedded does](https://developers.google.com/youtube/iframe_api_reference).

## What videos can be watched?
The same restrictions as IFrame api have. All videos that are not private, embeddable and not copyright withold from showing on other sites than youtube. 

## Under the hood.
A usercontrols that wraps a [CefSharp](https://github.com/cefsharp/CefSharp). The webcontrol contains a youtube embedded player which the usercontrol communicates with. 

# Requirments
See requirments of used [cefsharp](https://github.com/cefsharp/CefSharp/tree/master/NuGet)

## Usage
#### Reference
- x86 builds can use [nuge package](https://www.nuget.org/packages/galri.Csharp-YoutubePlayerControl):
- x64 builds must download source, compile, and reference dll's.
#### Bindings
See example project in solution.

## NB!
- As noted in the iframe api, trying to change the quality of a video, may be ignored by the iframeplayer.
- The build and debug must either be set to x86 or x64 because of CEF.
- From [youtube api](https://developers.google.com/youtube/iframe_api_reference#Requirements): "recommend 16:9 players be at least 480 pixels wide and 270 pixels tall."
