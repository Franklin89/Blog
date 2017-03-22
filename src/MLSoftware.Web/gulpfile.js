/// <binding AfterBuild='prepublish' Clean='clean' />

var gulp = require("gulp"),
    del = require("del"),
    concat = require("gulp-concat"),
    cssmin = require("gulp-cssmin"),
    uglify = require("gulp-uglify"),
    jshint = require('gulp-jshint'),
    csslint = require('gulp-csslint'),
    path = require("path");

var paths = {
    webroot: "./wwwroot/"
};

var library = {
    base: "node_modules",
    destination: "lib",
    source: [
        // glob pattern to get the dirname and match only js and min.js file wanted
        path.dirname(require.resolve('jquery-validation-unobtrusive/jquery.validate.unobtrusive.js')) + '/*unobtrusive**.js',
        // alternative of declaring each file
        require.resolve('bootstrap/dist/js/bootstrap.js'),
        require.resolve('bootstrap/dist/js/bootstrap.min.js'),
        require.resolve('bootstrap/dist/css/bootstrap.css'),
        require.resolve('bootstrap/dist/css/bootstrap.min.css'),
        require.resolve('font-awesome/css/font-awesome.css'),
        require.resolve('font-awesome/css/font-awesome.min.css'),  
        require.resolve('prismjs/themes/prism-okaidia.css'),
        // glob pattern to get all files within the directory
        path.dirname(require.resolve('bootstrap/dist/fonts/glyphicons-halflings-regular.woff')) + '/**',
        path.dirname(require.resolve('font-awesome/fonts/fontawesome-webfont.woff')) + '/**',
        // declare each file
        require.resolve('jquery/dist/jquery.js'),
        require.resolve('jquery/dist/jquery.min.js'),
        // only one file is distributed
        require.resolve('jquery-validation/dist/jquery.validate.js')
    ]
}

var prismjs = {
    base: "node_modules/prismjs",
    destination: "lib/prismjs",
    source: [        
        require.resolve('prismjs/components/prism-core.min.js'),
        require.resolve('prismjs/components/prism-markup.min.js'),
        require.resolve('prismjs/components/prism-css.min.js'),
        require.resolve('prismjs/components/prism-clike.min.js'),
        require.resolve('prismjs/components/prism-javascript.min.js'),
        require.resolve('prismjs/components/prism-csharp.min.js'),
        require.resolve('prismjs/components/prism-docker.min.js'),
        require.resolve('prismjs/components/prism-aspnet.min.js'),
        require.resolve('prismjs/components/prism-bash.min.js'),
        require.resolve('prismjs/plugins/file-highlight/prism-file-highlight.min.js')
    ]
}

paths.library = paths.webroot + library.destination;
paths.js = paths.webroot + "js/**/*.js";
paths.minJs = paths.webroot + "js/**/*.min.js";
paths.css = paths.webroot + "css/**/*.css";
paths.minCss = paths.webroot + "css/**/*.min.css";
paths.concatJsDest = paths.webroot + "js/site.min.js";
paths.concatCssDest = paths.webroot + "css/site.min.css";

gulp.task("lib", ["clean"], function () {
    return gulp.src(library.source, { base: library.base })
        .pipe(gulp.dest(paths.library));
});

gulp.task('build-prismjs', ["clean", "lib"], function () {
    return gulp.src(prismjs.source)
        .pipe(concat('prism.min.js'))
        .pipe(gulp.dest(paths.webroot + prismjs.destination));
});

gulp.task("clean:lib", function () {
    return del(paths.library);
});

gulp.task("clean:js", function () {
    return (del(paths.concatJsDest));
});

gulp.task("clean:css", function () {
    return (del(paths.concatCssDest));
});

gulp.task("clean", ["clean:js", "clean:css", "clean:lib"]);

gulp.task("min:js", function () {
    return gulp.src([paths.js, "!" + paths.minJs], { base: "." })
        .pipe(concat(paths.concatJsDest))
        .pipe(uglify())
        .pipe(gulp.dest("."));
});

gulp.task("min:css", function () {
    return gulp.src([paths.css, "!" + paths.minCss])
        .pipe(concat(paths.concatCssDest))
        .pipe(cssmin())
        .pipe(gulp.dest("."));
});

gulp.task("min", ["lib", "jshint", "min:js", "min:css"]);

gulp.task("jshint", ["lib"], function () {
    return gulp.src([paths.js, "!" + paths.minJs])
        .pipe(jshint())
        .pipe(jshint.reporter())
});

gulp.task("prepublish", ["lib", "jshint", "min", "build-prismjs"]);

gulp.task("default", ["prepublish"]);
