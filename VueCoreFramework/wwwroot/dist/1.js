webpackJsonp([1],{

/***/ 230:
/***/ (function(module, exports, __webpack_require__) {

var disposed = false
var Component = __webpack_require__(3)(
  /* script */
  null,
  /* template */
  __webpack_require__(232),
  /* styles */
  null,
  /* scopeId */
  null,
  /* moduleIdentifier (server only) */
  null
)
Component.options.__file = "C:\\Users\\wstead\\Documents\\Projects\\VueCoreFramework\\VueCoreFramework\\ClientApp\\components\\error\\notfound.vue"
if (Component.esModule && Object.keys(Component.esModule).some(function (key) {return key !== "default" && key.substr(0, 2) !== "__"})) {console.error("named exports are not supported in *.vue files.")}
if (Component.options.functional) {console.error("[vue-loader] notfound.vue: functional components are not supported with templates, they should use render functions.")}

/* hot reload */
if (true) {(function () {
  var hotAPI = __webpack_require__(1)
  hotAPI.install(__webpack_require__(2), false)
  if (!hotAPI.compatible) return
  module.hot.accept()
  if (!module.hot.data) {
    hotAPI.createRecord("data-v-090c010e", Component.options)
  } else {
    hotAPI.reload("data-v-090c010e", Component.options)
  }
  module.hot.dispose(function (data) {
    disposed = true
  })
})()}

module.exports = Component.exports


/***/ }),

/***/ 232:
/***/ (function(module, exports, __webpack_require__) {

module.exports={render:function (){var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;
  return _c('v-layout', {
    attrs: {
      "row": "",
      "wrap": ""
    }
  }, [_c('v-flex', {
    attrs: {
      "xs10": "",
      "offset-xs1": ""
    }
  }, [_c('h1', {
    staticClass: "error--text"
  }, [_vm._v("Not Found")]), _vm._v(" "), _c('h3', {
    staticClass: "error--text text--lighten-1"
  }, [_vm._v("Nothing was found here.")])])], 1)
},staticRenderFns: []}
module.exports.render._withStripped = true
if (true) {
  module.hot.accept()
  if (module.hot.data) {
     __webpack_require__(1).rerender("data-v-090c010e", module.exports)
  }
}

/***/ })

});
//# sourceMappingURL=1.js.map