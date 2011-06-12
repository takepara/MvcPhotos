$(function () {
    $("#debug").css({ opacity: 0.7 });
    debug("at SmartPhone.");

    $("#sendmail").show();

    var viewer = new PhotoViewer($("#contents"), $("#preview .image-container img"));
    viewer.previewSize = 0;
    viewer.clipZoom = [50, 25];
    viewer.clipSize = 50;
    var loader = new ModelLoader("#photos li", $("#tagsContainer").data("href"));

    var recentsUrl = $("ul.tags a:first").attr("href");
    var vm = {
        tags: ko.observableArray(),
        photos: ko.observableArray(),

        previewIndex: ko.observable(0),
        photosUrl: ko.observable(recentsUrl),
        photosLoadIndex: ko.observable(0),
        visibleCount: ko.observable(0),
        viewPhotos: function (elements) {
            $(".image-container", elements)
				    .css({
				        width: viewer.clipSize,
				        height: viewer.clipSize
				    })
				    .find("img")
					    .css({
					        width: viewer.imageSize(),
					        height: viewer.imageSize()
					    })
				    .end()
				    .show()
					.colorbox({ photo: true, initialWidth: 300, maxWidth: 300 });
        },
        viewPreview: function () { }
    };

    // events handling
    $("ul.tags a").live("click", function (e) {
        e.preventDefault();
        var url = $(this).attr("href");

        location.hash = url;
        loader.changeTag(vm, url);
        loader.load(viewer, vm);
    });

    // windows resize
    $(window).resize(function () {
        var len = vm.photos().length;
        vm.visibleCount(viewer.zoom(len));
    });

    var hashChanged = function () {
        viewer.visibleCount();
        if (location.hash === "") {
            $("#menu").fadeIn();
            $("#contents").fadeOut().hide();
        } else {
            $("#menu").fadeOut().hide();
            $("#contents").fadeIn();
        }
    };

    $(window).bind("hashchange", hashChanged);

    // view model binding
    ko.applyBindings(vm);

    // load photos
    setTimeout(function () {
        loader.load(viewer, vm);
        setTimeout(arguments.callee, 2000);
    }, 2000);

    $(window).trigger("resize");
    hashChanged();
    loader.load(viewer, vm);
});
