Title: Using Vue.js as a drop in component in you Razor Pages
Lead: 
Published: 01/20/2020 15:00
Tags: 
    - ASP.NET Core 3.1
    - Vue.js
    - Razor Pages
---

In this blog post I wanted to share how I am using [Vue.js](https://vuejs.org/) as a "drop in" component on a razor page. By drop in component I mean that I use Vue.js only for client side model binding on certain pages. This is similar to how we used to use Knockout.JS in ASP.NET MVC application. I will not explain what Vue.js is or what it does behind the scenes, I will show how I used Vue.js to create a paged list.

### Why Vue.js?

I am not a very big fan of SPA Frameworks like Angular or React and also in fact I am not a big fan of Vue.js for building full client side applications. In my opinion a server rendered page created with Razor Pages can be built in less time with less overhead and with a smaller chance of creating security issues. Never the less Vue.js is a great extension to such pages as it has great [documentation](https://vuejs.org/v2/guide/) and it is very simple to get it running on a single page where required.

### Including Vue.js to your application

Adding a reference to your application is very simple. If you have a scripts section defined in your layout this is all you will have to add to you razor page.

```razor
@section Scripts  {
    <script src="https://cdn.jsdelivr.net/npm/vue@2.6.11/dist/vue.min.js" asp-append-version="true"></script>
}
```

> You will have to check the docs for the latest link to make sure that you reference the version you require.

### Data Binding and beyond

Vue is really good for data binding and makes it really simple for us C# / ASP.NET Core developers to work and understand. First we need to create a new script and create a `Vue` JavaScript object.

```javascript
<script type="text/javascript">
    var app = new Vue({
            el: '#vueApp',
            data: {
                page: 1,
                pageSize: 5,
                eventItems: [],
                isLoading: true,
                noMoreData: false
            },
            computed: {
                showLoadMore: function () {
                    return !this.isLoading && !this.noMoreData
                }
            }
        });
    });
</script>
```

If we create a `Vue` object like above we target a DOM element with the id of `vueApp`. It contains some simple data and a computed variable. In the HTML part of the code we have to define this DOM element. It is nothing fancy, just a `div` with the correct id.

```html
<div id="vueApp">
    <!-- Content -->
</div>
```

Now we have to bind the `eventItems` form our Vue object to the HTML. Data binding is accomplished using custom attributes that begin with `v-` and with a Mustache syntax for text. You can also see that I am mixing Razor code `@((int)EventType.Training)` with some JavaScript to show / hide certain parts.

```html
<div id="vueApp">
    <div class="row">
        <div class="col-12 col-lg-8 offset-lg-2">
            <div id="events" class="list-group">
                <div v-for="eventItem in eventItems" class="list-group-item mb-3 event">
                    <!-- Content -->
                    <div class="d-flex justify-content-start" v-on:click="openEventDetails(eventItem.id, $event)">
                        <!-- Icon -->
                        <img v-else :src="eventItem.icon" class="p-2 news-icon">
                        <div class="ml-2">
                            <!-- Title -->
                            <h5 v-if="eventItem.type === @((int)EventType.Training)">@Localizer["#Training"]</h5>
                            <h5 v-else>{{ eventItem.title }}</h5>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- Load more button -->
    <div v-if="showLoadMore" class="d-flex justify-content-center">
        <button class="btn btn-outline-info" v-on:click="loadEvents">
            <span>Load More...</span>
        </button>
    </div>
</div>
```

### Getting it to work

If you run the application now, nothing will be displayed, because the `eventItems` array is still empty. So we have to add some functionality to load the data from the backend in to this array. We extent our Vue object with a `methods` variable, in this case with a function called `loadEvents`. This function calls an `ApiController` on our backend using the jQuery `$.getJson` function. We check if we got any data back and if so we add it to the `eventItems` array. We also want to load the first page of events as soon as the page loads. For this Vue.js offers some lifecycle events. One of which is `mounted`, this gets called when this Vue object gets loaded into memory (so when it is executed).

```javascript
<script type="text/javascript">
    $(function() {
        var app = new Vue({
                el: '#vueApp',
                data: {
                    page: 1,
                    pageSize: 5,
                    eventItems: [],
                    noMoreData: false
                },
                computed: {
                    showLoadMore: function () {
                        return !this.noMoreData
                    }
                },
                methods: {
                    loadEvents: function () {
                        console.log(`Load events page: ${this.page}`);

                        $.getJSON(`/api/events?pageNumber=${this.page}&pageSize=${this.pageSize}`, function (data) {
                            if (data.length == 0) {
                                console.log('No more data');
                                this.noMoreData = true;
                            }
                            else {
                                this.eventItems = this.eventItems.concat(data);
                            }

                            this.page += 1;
                        }.bind(this));
                    }
                },
                mounted: function () {
                    this.loadEvents();
                }
            });
    });
</script>
```

If we run the application now, we get a nice paging list group that we can then keep enhancing. For example we could add a spinner that gets displayed while loading the data and so on.

### Summary

In this blog post I tried to show how easy and simple you can use Vue.js in your ASP.NET Core Application as a simple 'drop-in' component for some client side logic. This can also be done by using jQuery or pure JavaScript, but in my opinion this is very lightweight and also it is very clean.

If you like this blog post drop a comment or buy me a coffee at the bottom of the page <i class="fa fa-coffee"></i>