webpackJsonp([0],{

/***/ 229:
/***/ (function(module, exports, __webpack_require__) {

var disposed = false
var Component = __webpack_require__(3)(
  /* script */
  __webpack_require__(231),
  /* template */
  __webpack_require__(233),
  /* styles */
  null,
  /* scopeId */
  null,
  /* moduleIdentifier (server only) */
  null
)
Component.options.__file = "C:\\Users\\wstead\\Documents\\Projects\\VueCoreFramework\\VueCoreFramework\\ClientApp\\components\\error\\error.vue"
if (Component.esModule && Object.keys(Component.esModule).some(function (key) {return key !== "default" && key.substr(0, 2) !== "__"})) {console.error("named exports are not supported in *.vue files.")}
if (Component.options.functional) {console.error("[vue-loader] error.vue: functional components are not supported with templates, they should use render functions.")}

/* hot reload */
if (true) {(function () {
  var hotAPI = __webpack_require__(1)
  hotAPI.install(__webpack_require__(2), false)
  if (!hotAPI.compatible) return
  module.hot.accept()
  if (!module.hot.data) {
    hotAPI.createRecord("data-v-60cdc6ce", Component.options)
  } else {
    hotAPI.reload("data-v-60cdc6ce", Component.options)
  }
  module.hot.dispose(function (data) {
    disposed = true
  })
})()}

module.exports = Component.exports


/***/ }),

/***/ 231:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
Object.defineProperty(__webpack_exports__, "__esModule", { value: true });
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0_vue__ = __webpack_require__(2);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1_vue_property_decorator__ = __webpack_require__(5);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1_vue_property_decorator___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_1_vue_property_decorator__);
var __extends = (this && this.__extends) || (function () {
    var extendStatics = Object.setPrototypeOf ||
        ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
        function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};


var ErrorComponent = (function (_super) {
    __extends(ErrorComponent, _super);
    function ErrorComponent() {
        var _this = _super !== null && _super.apply(this, arguments) || this;
        _this.errorMsg = '';
        return _this;
    }
    ErrorComponent.prototype.mounted = function () {
        switch (this.code) {
            case '400':
                this.errorMsg = "Your request was invalid.";
                break;
            case '401':
                this.errorMsg = "You don't have permission to access this page.";
                break;
            case '403':
                this.errorMsg = "You don't have access to this page.";
                break;
            case '404':
                this.errorMsg = "Nothing was found here.";
                break;
            case '418':
                this.errorMsg = "I'm a teapot.";
                break;
        }
    };
    return ErrorComponent;
}(__WEBPACK_IMPORTED_MODULE_0_vue__["default"]));
__decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_1_vue_property_decorator__["Prop"])()
], ErrorComponent.prototype, "code", void 0);
ErrorComponent = __decorate([
    __WEBPACK_IMPORTED_MODULE_1_vue_property_decorator__["Component"]
], ErrorComponent);
/* harmony default export */ __webpack_exports__["default"] = (ErrorComponent);


/***/ }),

/***/ 233:
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
  }, [_vm._v("Error " + _vm._s(_vm.code))]), _vm._v(" "), _c('h3', {
    staticClass: "error--text text--lighten-1"
  }, [_vm._v(_vm._s(_vm.errorMsg || 'An error occurred while processing your request.'))])])], 1)
},staticRenderFns: []}
module.exports.render._withStripped = true
if (true) {
  module.hot.accept()
  if (module.hot.data) {
     __webpack_require__(1).rerender("data-v-60cdc6ce", module.exports)
  }
}

/***/ })

});
//# sourceMappingURL=0.js.map