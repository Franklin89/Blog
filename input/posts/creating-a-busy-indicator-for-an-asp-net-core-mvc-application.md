Title: Creating a busy indicator for an ASP.NET Core MVC Application
Published: 10/28/2018
Tags: 
    - MVC
    - ASP.NET Core 2.1
    - UX
RedirectFrom: blog/creating-a-busy-indicator-for-an-asp-net-core-mvc-application
---

It is essential to a good user experience to give feedback to the user if the application is processing a request. We know this scenario from single page applications or while reloading partial parts of a website. But what if we have a classic server rendered web application like a ASP.NET Core MVC web application? In this blog post I will show a very simple way you can display a busy indicator for your ASP.NET Core MVC application.

### Creating _BusyIndicatoPartial.cshtml
Keeping it very simple at the moment. This can be enhanced to any extend you want.

```html
<div class="loading">
  Loading&#8230;
</div>
```

### Styling the busy indicator
The busy indicator will overlay everything with a light gray overlay and a spinner at the center of the screen. The animation is done purely in css. If you want you can change this with a gif of some sort.
```css
/* Absolute Center Spinner */
.loading {
  position: fixed;
  display: none;
  z-index: 1031;
  height: 2em;
  width: 2em;
  overflow: show;
  margin: auto;
  top: 0;
  left: 0;
  bottom: 0;
  right: 0;
}

/* Transparent Overlay */
.loading:before {
  content: '';
  display: block;
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0,0,0,0.3);
}

/* :not(:required) hides these rules from IE9 and below */
.loading:not(:required) {
  font: 0/0 a;
  color: transparent;
  text-shadow: none;
  background-color: transparent;
  border: 0;
}

.loading:not(:required):after {
  content: '';
  display: block;
  font-size: 10px;
  width: 1em;
  height: 1em;
  margin-top: -0.5em;
  -webkit-animation: spinner 1500ms infinite linear;
  -moz-animation: spinner 1500ms infinite linear;
  -ms-animation: spinner 1500ms infinite linear;
  -o-animation: spinner 1500ms infinite linear;
  animation: spinner 1500ms infinite linear;
  border-radius: 0.5em;
  -webkit-box-shadow: rgba(0, 0, 0, 0.75) 1.5em 0 0 0, rgba(0, 0, 0, 0.75) 1.1em 1.1em 0 0, rgba(0, 0, 0, 0.75) 0 1.5em 0 0, rgba(0, 0, 0, 0.75) -1.1em 1.1em 0 0, rgba(0, 0, 0, 0.5) -1.5em 0 0 0, rgba(0, 0, 0, 0.5) -1.1em -1.1em 0 0, rgba(0, 0, 0, 0.75) 0 -1.5em 0 0, rgba(0, 0, 0, 0.75) 1.1em -1.1em 0 0;
  box-shadow: rgba(0, 0, 0, 0.75) 1.5em 0 0 0, rgba(0, 0, 0, 0.75) 1.1em 1.1em 0 0, rgba(0, 0, 0, 0.75) 0 1.5em 0 0, rgba(0, 0, 0, 0.75) -1.1em 1.1em 0 0, rgba(0, 0, 0, 0.75) -1.5em 0 0 0, rgba(0, 0, 0, 0.75) -1.1em -1.1em 0 0, rgba(0, 0, 0, 0.75) 0 -1.5em 0 0, rgba(0, 0, 0, 0.75) 1.1em -1.1em 0 0;
}

/* Animation */
@-webkit-keyframes spinner {
  0% {
    -webkit-transform: rotate(0deg);
    -moz-transform: rotate(0deg);
    -ms-transform: rotate(0deg);
    -o-transform: rotate(0deg);
    transform: rotate(0deg);
  }
  100% {
    -webkit-transform: rotate(360deg);
    -moz-transform: rotate(360deg);
    -ms-transform: rotate(360deg);
    -o-transform: rotate(360deg);
    transform: rotate(360deg);
  }
}
@-moz-keyframes spinner {
  0% {
    -webkit-transform: rotate(0deg);
    -moz-transform: rotate(0deg);
    -ms-transform: rotate(0deg);
    -o-transform: rotate(0deg);
    transform: rotate(0deg);
  }
  100% {
    -webkit-transform: rotate(360deg);
    -moz-transform: rotate(360deg);
    -ms-transform: rotate(360deg);
    -o-transform: rotate(360deg);
    transform: rotate(360deg);
  }
}
@-o-keyframes spinner {
  0% {
    -webkit-transform: rotate(0deg);
    -moz-transform: rotate(0deg);
    -ms-transform: rotate(0deg);
    -o-transform: rotate(0deg);
    transform: rotate(0deg);
  }
  100% {
    -webkit-transform: rotate(360deg);
    -moz-transform: rotate(360deg);
    -ms-transform: rotate(360deg);
    -o-transform: rotate(360deg);
    transform: rotate(360deg);
  }
}
@keyframes spinner {
  0% {
    -webkit-transform: rotate(0deg);
    -moz-transform: rotate(0deg);
    -ms-transform: rotate(0deg);
    -o-transform: rotate(0deg);
    transform: rotate(0deg);
  }
  100% {
    -webkit-transform: rotate(360deg);
    -moz-transform: rotate(360deg);
    -ms-transform: rotate(360deg);
    -o-transform: rotate(360deg);
    transform: rotate(360deg);
  }
}
```

### Update the _Layout.cshtml
Add the partial view to the `_Layout.cshtml`.

```html
<body>
    <partial name="_BusyIndicatorPartial" />
	
    ...
	
</body>
```

### Add some javascript to display the busy indicator
To display the busy indicator we need some client side code that can execute and display the busy indicator if required. In my scenario I needed to display the busy indicator if the user navigates to a different page or if the user submits a form and waits for the data to be saved. This can be done in a very generic way, so no extra code will be needed on every page. 

#### Display the busy indicator
```js
function displayBusyIndicator() {
    $('.loading').show();
}
```

#### Navigation
We are listening on the window event `beforeunload`. In any case, if the user navigates away the busy indicator will be displayed. *The event `beforeunload` is not supported on all browsers!*

```js
$(window).on('beforeunload', function(){
    displayBusyIndicator();
});
```

#### Form Submission
There is an event on the the document that will be fired if a form is submitted. Hooked up to this event will result in the effect that on every submission of a form the busy indicator will be displayed.

```js
$(document).on('submit', 'form', function () {
    displayBusyIndicator();
});
```

### Summary
In this blog post I showed a very simple way of creating a busy indicator in your ASP.NET Core MVC application. The busy indicator will give some feedback to the user about what is happening. But there are also other benefits out of this. If you have long running tasks the user will not try to post back again, he actually can't because of the gray overlay. Also if you have users that double click the submit button, your form will not get submitted twice. So not only the user benefits from this, but also your application. I am sure there are many ways of implementing this, but I thought this was a very simple way that gets you started and fits my use case up until now.