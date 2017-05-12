/*!
 * Vue-Responsive v0.1.5
 * @Url: https://github.com/reinerBa/Vue-Responsive
 * @License: MIT, Reiner Bamberger
 */

/*
 * Adapted with class support, using some code from the public feature/classSupport branch,
 * but with a slightly different implementation.
 * -- Wil Stead
 */

export default {
    bind: function (el, binding, vnode) {
        let self = vnode;
        //Bootstrap 4 Repsonsive Utils default
        var componentDefault = undefined;
        if (!self.__rPermissions) {
            self.__rPermissions = {};
            self.__rPermissions.bs4 = { xs: { show: true, min: -1, max: 543 }, sm: { show: true, min: 544, max: 767 }, md: { show: true, min: 768, max: 991 }, lg: { show: true, min: 992, max: 1199 }, xl: { show: true, min: 1200, max: Infinity } };
            //:bs3
            self.__rPermissions.bs3 = { xs: { show: true, min: -1, max: 767 }, sm: { show: true, min: 768, max: 991 }, md: { show: true, min: 992, max: 1199 }, lg: { show: true, min: 1200, max: Infinity } };
            for (var i in vnode.context.$data)
                if (i.indexOf("responsiveMarks$$") === 0) {
                    let name = String(i).replace("responsiveMarks$$", "").toLowerCase();
                    self.__rPermissions[name] = {};
                    for (let ii in vnode.context.$data[i]) self.__rPermissions[name][ii] = vnode.context.$data[i][ii];
                }
            if (i === "responsiveDefault$$") componentDefault = vnode.context.$data[i];
            self.__rPermissions.undefined = componentDefault ? self.__rPermissions[componentDefault] : self.__rPermissions.bs4;
        }
        var validInputs = ['hidden-all'];
        for (let i in self.__rPermissions[binding.arg]) {
            validInputs.push(i);
            validInputs.push("hidden-" + i);
        }

        //to use just one resize-event-listener whoose functions can easy unbound
        self.__rIntervalInstId = ++self.__rIntervalInstId || 1;
        var rPermissions = { rId: String(self.__rIntervalInstId) };
        function callInstances() {
            for (let i in self.resizeListeners)
                if (!isNaN(parseInt(i)))
                    self.resizeListeners[i]();
        }
        if (typeof self.resizeListeners === 'undefined') {
            self.resizeListeners = {};
            window.addEventListener("resize", callInstances);
        }

        var preParams = [];

        if (Array.isArray(binding.value) || (typeof binding.expression === "string" && !!binding.expression.match(/\[*\]/))) {
            if (Array.isArray(binding.value))
                preParams = binding.value;
            else {
                let stringArray = binding.expression.replace(/'/g, '"');
                preParams = JSON.parse(stringArray);
            }
            preParams.sort();
        } else if (typeof binding.value === 'object') {
            for (let i in binding.value) {
                if (binding.value[i]) preParams.push(i);
            }
        } else if (typeof binding.value === "string" || typeof binding.expression === "string") {   //a single parameter
            let val = binding.value || binding.expression.replace(/'"/g, "");
            preParams = new Array(val);
            preParams.sort();
        } else {
            preParams = null;
            return false;
        }
        if (!preParams) return 0;

        for (let k in self.__rPermissions[binding.arg]) {
            rPermissions[k] = true;
        }

        if (preParams[0] === "hidden-all") {
            preParams.splice(0, 1);
            for (let i in self.__rPermissions[binding.arg]) {
                rPermissions[i] = false;
            }
        }

        let c = 0, item;
        while (item = preParams[c++]) {
            if (validInputs.indexOf(item) != -1) {
                if (item.indexOf("hidden") === 0) { //hidden-..
                    let key = item.split("-")[1];
                    rPermissions[key] = false;
                } else {
                    rPermissions[item] = true;
                }
            }
        }
        el.dataset.responsives = JSON.stringify(rPermissions);
    },
    inserted: function (el, binding, vnode) {
        if (el.dataset.responsives == null) return 0;
        let self = vnode;

        function checkDisplay() {
            var myPermissions = JSON.parse(el.dataset.responsives);
            var curWidth = window.innerWidth;
            for (let i in self.__rPermissions[binding.arg]) {
                if (curWidth >= self.__rPermissions[binding.arg][i].min && curWidth <= self.__rPermissions[binding.arg][i].max) {
                    if (myPermissions[i]) {
                        if (el.classList.contains("vResponsiveHidden")) {
                            el.classList.remove("vResponsiveHidden");
                        }
                    }
                    else if (!el.classList.contains("vResponsiveHidden")) {
                        el.classList.add("vResponsiveHidden");
                    }
                    break;
                }
            }
        };
        checkDisplay();

        let resizeListenerId = JSON.parse(el.dataset.responsives).rId;
        self.resizeListeners[resizeListenerId] = checkDisplay;
    },
    unbind: function (el, binding, vnode) {
        let self = vnode;
        let resizeListenerId = JSON.parse(el.dataset.responsives).rId;
        delete self.resizeListeners[resizeListenerId];
    }
};

