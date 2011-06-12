// $.toJSONは内部でJSON.parseやstringifyとかのNativeメソッドが
// 使えるなら使うから賢いよ。

function debug(value) {
    if (value.prototype === {}.prototype &&
			typeof $.toJSON === typeof Function)
        value = $.toJSON(value);

    $("#debug").text(new Date().toString() + " - " + value);
}

var Class = {
    create: function () {
        return function () {
            this.init.apply(this, arguments);
        }
    }
}

// ajax

var ModelLoader = Class.create();
ModelLoader.prototype = {
    _thumbsSelector: "",
    _tagsUrl: "",
    _prevTime: new Date(),
    _prevUrl: null,
    init: function (thumbsSelector, tagsUrl) {
        this._thumbsSelector = thumbsSelector;
        this._tagsUrl = tagsUrl;
    },
    changeTag: function (vm, url) {
        vm.photosUrl(url);
        vm.previewIndex(0);
        vm.photosLoadIndex(0);
        vm.photos([]);
        this._prevUrl = null;
    },
    load: function (viewer, vm, preview) {
        // delay control
        var now = new Date();
        if ((now - this._prevTime) < 2100 && this._prevUrl === vm.photosUrl()) {
            return;
        }
        this._prevUrl = vm.photosUrl();
        this._prevTime = now;

        // logic
        var self = this;
        var last = vm.photosLoadIndex();
        var parameters = {};
        if (last) {
            parameters.last = last;
        } else {
            vm.photosLoadIndex(0);
            vm.photos([]);
        }
        $.getJSON(vm.photosUrl(), parameters, function (model) {
            var data = model.Items;

            var len = vm.photos().length;
            if (viewer.visibleCount() < (len + data.length)) {
                vm.visibleCount(viewer.zoom(len));
                vm.viewPhotos($(self._thumbsSelector));
            }

            if (data.length) {
                vm.photosLoadIndex(data[0].Id);
                data = data.reverse();
                for (var i = 0; i < data.length; i++) {
                    var over = vm.photos().length - viewer.visibleCount();
                    while (over >= 0) {
                        vm.photos.pop();
                        over--;
                    }
                    vm.photos.unshift(data[i]);
                }

                // preview delegate
                if (typeof preview === typeof Function) {
                    preview(vm);
                }
            }
        });

        $.getJSON(self._tagsUrl, function (model) {
            var data = model.Items;
            vm.tags(data);
        });
    }
};

// photos

var PhotoViewer = Class.create();
PhotoViewer.prototype = {
    _contents: null,
    _preview: null,

    previewSize: 400,
    clipZoom: [100, 50, 25],
    clipSize: 100,

    init: function (contents, preview) {
        this._contents = contents;
        this._preview = preview;
        $(preview).parent().css({
            width: this.previewSize,
            height: this.previewSize
        });
    },

    imageSize: function (contents) {
        var resize = { 400: 350, 100: 90, 50: 45, 25: 22 };

        return resize[this.clipSize];
    },

    visibleCount: function (size) {
        size = size || this.clipSize;
        var w = $(this._contents).width() / size;
        var h = $(this._contents).height() / size;
        var count = Math.floor(w) * Math.floor(h);

        if (this.previewSize) {
            count -= Math.pow(Math.ceil((this.previewSize) / size), 2);
        }

        if (isNaN(count))
            count = 0;
        return Math.max(count, 0);
    },

    zoom: function (visible) {
        visible = visible || 0;
        var fit = 0;
        for (var i = 0; i < this.clipZoom.length; i++) {
            var fit = this.visibleCount(this.clipZoom[i]);
            if (visible < fit) {
                this.clipSize = this.clipZoom[i];
                break;
            }
        }

        return fit;
    },

    preview: function (anchor) {
        var self = this;
        var url = $(anchor).attr("href");
        var img = new Image();
        $(img).load(function () {
            $(self._preview)
				.fadeOut()
				.queue(function () {
				    $(this).attr("src", url);
				    $(this).dequeue();
				})
				.fadeIn();
        });
        img.src = url;
    },

    destroy: function () {

    }
};
