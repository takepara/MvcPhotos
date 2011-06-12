$(function () {
    $("#debug").css({ opacity: 0.7 });

    if (Modernizr.touch) {
        debug("at iPad.");
        $("#sendmail").show();
    } else {
        debug("at PC.");
        $("#upload").show().colorbox({ iframe: true, width: 500, height: 500 });
    }
    $("#preview").colorbox({ photo: true });

    var viewer = new PhotoViewer($("#contents"), $("#preview .image-container img"));
    var loader = new ModelLoader("#photos li", $("#tagsContainer").data("href"));
    var preview = function (dvm) {
        $("#photos .image-container").css({ opacity: 0.5 });
        $("#photos .image-container:eq(" + dvm.previewIndex() + ")").css({ opacity: 1 });

        dvm.viewPreview();
    };

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
				.show();
        },
        viewPreview: function () {
            $("#preview img").hide();
            if (!vm.photos().length) {
                return;
            }
            var element = $("#photos .image-container:eq(" + vm.previewIndex() + ")")[0];
            var src = $(element).attr("href");
            $("#preview").attr("href", src.split('?')[0]);
            // preview はわけあって少し遅れて表示するように。
            setTimeout(function () {
                var img = new Image();
                $(img).load(function () {
                    $("#preview img").attr("src", src).fadeIn();
                });
                img.src = src;
            }, 500);
        }
    };

    // events handling
    $("#preview img").load(function () {
        $(this).fadeIn();
    });
    $("#photos .image-container").live("click", function (e) {
        e.preventDefault();
        $("#photos .image-container").css({ opacity: 0.5 });
        $(this).css({ opacity: 1 });

        vm.previewIndex($("#photos .image-container").index(this));
        vm.viewPreview();
    });
    $("#photos li").live("mouseover", function () {
        $(this).find("img").addClass("bordered");
    });
    $("#photos li").live("mouseout", function () {
        $(this).find("img").removeClass("bordered");
    });
    $("ul.tags a").live("click", function (e) {
        e.preventDefault();
        var url = $(this).attr("href");

        loader.changeTag(vm, url);
        loader.load(viewer, vm, preview);
    });

    // windows resize
    $(window).resize(function () {
        $("#tagsContainer").css({ height: $("#menu").height() - 45 });
        var len = vm.photos().length;
        vm.visibleCount(viewer.zoom(len));
        vm.viewPhotos($("#photos li"));
    });

    // view model binding
    ko.applyBindings(vm);

    // load photos
    setTimeout(function () {
        loader.load(viewer, vm, preview);
        setTimeout(arguments.callee, 2000);
    }, 2000);

    $(window).trigger("resize");
    $("#recents").trigger("click");
});
