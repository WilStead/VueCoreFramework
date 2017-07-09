// adapted from http://www.localeplanet.com/api/translate.js

interface FormatterFunction {
    (s: string, ...args: string[]): string;
}

//from https://gist.github.com/776196 via http://davedash.com/2010/11/19/pythonic-string-formatting-in-javascript/
let defaultFormatter: FormatterFunction = (function () {
    let re = /\{([^}]+)\}/g;
    return function (s: string, ...args: string[]) {
        return s.replace(re, function (_, match) { return args[match]; });
    }
}());
let formatter = defaultFormatter;

interface SetFormatterFunction {
    (newFormatter: FormatterFunction): void;
}

interface TranslateFunction {
    (map, text: string, ...path: string[]): string;
}

interface UnmappedTranslateFunction {
    (text: string, ...path: string[]): string;
}

let dynoTrans: UnmappedTranslateFunction = null;

interface SetDynamicTranslatorFunction {
    (newDynoTrans: UnmappedTranslateFunction): void;
}

let translation = null;

interface SetTranslationFunction {
    (newTranslation: any): void;
}

function translateLookup(map, target: string, ...path: string[]): string | TranslateFunction {
    if (map == null || target == null) {
        return target;
    }

    if (path.length > 0) {
        if (path[0] in map == false) {
            if (dynoTrans != null) {
                return dynoTrans(target);
            }
            return target;
        }
        return translateLookup(map[path[0]], target, ...path.slice(1));
    }

    if (target in map == false) {
        if (dynoTrans != null) {
            return dynoTrans(target);
        }
        return target;
    }

    var result = map[target];
    if (result == null) {
        return target;
    }

    return result;
};

interface Translator extends UnmappedTranslateFunction {
    translate: TranslateFunction;
    setFormatter: SetFormatterFunction;
    format: FormatterFunction;
    setDynamicTranslator: SetDynamicTranslatorFunction;
    setTranslation: SetTranslationFunction;
}

function getTranslator(): Translator {
    let translate = <Translator>function (text: string, ...path: string[]): string {
        let xlate = translateLookup(translation, text, ...path);

        if (typeof xlate === "function") {
            xlate = xlate.apply(this, arguments) as string;
        } else if (arguments.length > 1) {
            let aps = Array.prototype.slice;
            let args = aps.call(arguments, 1);

            xlate = formatter(xlate, args);
        }

        return xlate as string;
    };

    // Available explicity as well as via the object
    translate.translate = translate;

    translate.setFormatter = function (newFormatter) {
        formatter = newFormatter;
    };

    translate.format = function (s: string, ...args: string[]) {
        return formatter(s, ...args);
    };

    translate.setDynamicTranslator = function (newDynoTrans) {
        dynoTrans = newDynoTrans;
    };

    translate.setTranslation = function (newTranslation) {
        translation = newTranslation;
    };

    return translate;
}

let _t = getTranslator();
export default _t;
