var text = {
    defaultToken: '',
    tokenPostfix: '.cs',

    brackets: [
        { open: '{', close: '}', token: 'delimiter.curly' },
        { open: '[', close: ']', token: 'delimiter.square' },
        { open: '(', close: ')', token: 'delimiter.parenthesis' },
        { open: '<', close: '>', token: 'delimiter.angle' }
    ],

    keywords: [
        'extern', 'alias', 'using', 'bool', 'decimal', 'sbyte', 'byte', 'short',
        'ushort', 'int', 'uint', 'long', 'ulong', 'char', 'float', 'double',
        'object', 'dynamic', 'string', 'assembly', 'is', 'as', 'ref',
        'out', 'this', 'base', 'new', 'typeof', 'void', 'checked', 'unchecked',
        'default', 'delegate', 'var', 'const', 'if', 'else', 'switch', 'case',
        'while', 'do', 'for', 'foreach', 'in', 'break', 'continue', 'goto',
        'return', 'throw', 'try', 'catch', 'finally', 'lock', 'yield', 'from',
        'let', 'where', 'join', 'on', 'equals', 'into', 'orderby', 'ascending',
        'descending', 'select', 'group', 'by', 'namespace', 'partial', 'class',
        'field', 'event', 'method', 'param', 'property', 'public', 'protected',
        'internal', 'private', 'abstract', 'sealed', 'static', 'struct', 'readonly',
        'volatile', 'virtual', 'override', 'params', 'get', 'set', 'add', 'remove',
        'operator', 'true', 'false', 'implicit', 'explicit', 'interface', 'enum',
        'null', 'async', 'await', 'fixed', 'sizeof', 'stackalloc', 'unsafe', 'nameof',
        'when'
    ],

    namespaceFollows: [
        'namespace', 'using',
    ],

    parenFollows: [
        'if', 'for', 'while', 'switch', 'foreach', 'using', 'catch', 'when'
    ],

    operators: [
        '=', '??', '||', '&&', '|', '^', '&', '==', '!=', '<=', '>=', '<<',
        '+', '-', '*', '/', '%', '!', '~', '++', '--', '+=',
        '-=', '*=', '/=', '%=', '&=', '|=', '^=', '<<=', '>>=', '>>', '=>'
    ],

    symbols: /[=><!~?:&|+\-*\/\^%]+/,

    // escape sequences
    escapes: /\\(?:[abfnrtv\\"']|x[0-9A-Fa-f]{1,4}|u[0-9A-Fa-f]{4}|U[0-9A-Fa-f]{8})/,

    // The main tokenizer for our languages
    tokenizer: {
        root: [

            // identifiers and keywords
            [/\@?[a-zA-Z_]\w*/, {
                cases: {
                    '@namespaceFollows': { token: 'keyword.$0', next: '@namespace' },
                    '@keywords': { token: 'keyword.$0', next: '@qualified' },
                    '@default': { token: 'identifier', next: '@qualified' }
                }
            }],

            // whitespace
            { include: '@whitespace' },

            // delimiters and operators
            [/}/, {
                cases: {
                    '$S2==interpolatedstring': { token: 'string.quote', next: '@pop' },
                    '$S2==litinterpstring': { token: 'string.quote', next: '@pop' },
                    '@default': '@brackets'
                }
            }],
            [/[{}()\[\]]/, '@brackets'],
            [/[<>](?!@symbols)/, '@brackets'],
            [/@symbols/, {
                cases: {
                    '@operators': 'delimiter',
                    '@default': ''
                }
            }],


            // numbers
            [/[0-9_]*\.[0-9_]+([eE][\-+]?\d+)?[fFdD]?/, 'number.float'],
            [/0[xX][0-9a-fA-F_]+/, 'number.hex'],
            [/0[bB][01_]+/, 'number.hex'], // binary: use same theme style as hex
            [/[0-9_]+/, 'number'],

            // delimiter: after number because of .\d floats
            [/[;,.]/, 'delimiter'],

            // strings
            [/"([^"\\]|\\.)*$/, 'string.invalid'],  // non-teminated string
            [/"/, { token: 'string.quote', next: '@string' }],
            [/\$\@"/, { token: 'string.quote', next: '@litinterpstring' }],
            [/\@"/, { token: 'string.quote', next: '@litstring' }],
            [/\$"/, { token: 'string.quote', next: '@interpolatedstring' }],

            // characters
            [/'[^\\']'/, 'string'],
            [/(')(@escapes)(')/, ['string', 'string.escape', 'string']],
            [/'/, 'string.invalid']
        ],

        qualified: [
            [/[a-zA-Z_][\w]*/, {
                cases: {
                    '@keywords': { token: 'keyword.$0' },
                    '@default': 'identifier'
                }
            }],
            [/\./, 'delimiter'],
            ['', '', '@pop'],
        ],

        namespace: [
            { include: '@whitespace' },
            [/[A-Z]\w*/, 'namespace'],
            [/[\.=]/, 'delimiter'],
            ['', '', '@pop'],
        ],

        comment: [
            [/[^\/*]+/, 'comment'],
            // [/\/\*/,    'comment', '@push' ],    // no nested comments :-(
            ['\\*/', 'comment', '@pop'],
            [/[\/*]/, 'comment']
        ],

        string: [
            [/[^\\"]+/, 'string'],
            [/@escapes/, 'string.escape'],
            [/\\./, 'string.escape.invalid'],
            [/"/, { token: 'string.quote', next: '@pop' }]
        ],

        litstring: [
            [/[^"]+/, 'string'],
            [/""/, 'string.escape'],
            [/"/, { token: 'string.quote', next: '@pop' }]
        ],

        litinterpstring: [
            [/[^"{]+/, 'string'],
            [/""/, 'string.escape'],
            [/{{/, 'string.escape'],
            [/}}/, 'string.escape'],
            [/{/, { token: 'string.quote', next: 'root.litinterpstring' }],
            [/"/, { token: 'string.quote', next: '@pop' }]
        ],

        interpolatedstring: [
            [/[^\\"{]+/, 'string'],
            [/@escapes/, 'string.escape'],
            [/\\./, 'string.escape.invalid'],
            [/{{/, 'string.escape'],
            [/}}/, 'string.escape'],
            [/{/, { token: 'string.quote', next: 'root.interpolatedstring' }],
            [/"/, { token: 'string.quote', next: '@pop' }]
        ],

        whitespace: [
            [/^[ \t\v\f]*#((r)|(load))(?=\s)/, 'directive.csx'],
            [/^[ \t\v\f]*#\w.*$/, 'namespace.cpp'],
            [/[ \t\v\f\r\n]+/, ''],
            [/\/\*/, 'comment', '@comment'],
            [/\/\/.*$/, 'comment'],
        ],
    },
};

var ed;
var edmodel;
require.config({ paths: { 'vs': 'https://unpkg.com/monaco-editor@0.8.3/min/vs' } });
window.MonacoEnvironment = {
    getWorkerUrl: () => proxy
};

var languageId = 'monarch-language-csharp';

let proxy = URL.createObjectURL(new Blob([` self.MonacoEnvironment = {baseUrl: 'https://unpkg.com/monaco-editor@0.8.3/min/' }; importScripts('https://unpkg.com/monaco-editor@0.8.3/min/vs/base/worker/workerMain.js'); `], { type: 'text/javascript' }));

require(["vs/editor/editor.main"], function () {



    monaco.languages.registerCompletionItemProvider('csharp', getcsharpCompletionProvider(monaco));

    monaco.editor.defineTheme('myCustomTheme', {
        base: 'vs-dark', // can also be vs-dark or hc-black
        inherit: true, // can also be false to completely replace the builtin rules

        rules: [

            { token: 'comment', foreground: 'ffa500', fontStyle: 'italic underline' },
            { token: 'comment.js', foreground: '008800', fontStyle: 'bold' },
            { token: 'comment.css', foreground: '0000ff' }, // will inherit fontStyle from `comment` above
            { token: 'comment.csharp', foreground: '0000ff' },
            { token: 'class.csharp', foreground: '0000ff' },
            { token: 'type', foreground: 'ff0000' },
            { token: ' entity.name.function', foreground: 'ff0000' },

            {
                token: "entity.name.class", foreground: 'ff0000'
            },
            {
                token: "entity.name.type", foreground: 'ff0000'
            },
            {
                token: "entity.name.namespace", foreground: 'ff0000'
            },
            { token: "entity.name.scope-resolution", foreground: 'ff0000' },
        ]
    });


    monaco.languages.setLanguageConfiguration('csharp', {
        indentationRules: {

            decreaseIndentPattern: /^((?!.*?\/\*).*\*\/)?\s*[\}\]\)].*$/,

            increaseIndentPattern: /^((?!\/\/).)*(\{[^}"'`]*|\([^)"'`]*|\[[^\]"'`]*)$/
        }
    });


    let editor = monaco.editor.create(document.getElementById('monaco'), {
        value: [
            `using System;
using System.Net.Http;

public class Program
{
        public static void Main()
                            {
        Console.WriteLine("Hello World");
    var te=new test();
    te.settings="";
    var tc= new TClass();
    var client= new HttpClient();
client.
}

                            public  class test{
        public string version {get;set;}
                                public string settings {get;set;}
                                public TClass tdetails {get;set;}
}

                            public  class TClass{
        public string tpropone {get;set;}
}

}`
        ].join('\n'),
        language: 'csharp',
        suggestOnTriggerCharacters: true,
        //theme: 'vs-dark',
        theme: "myCustomTheme",
        minimap: {
            enabled: true
        },
        formatOnType: true,
        formatOnPaste: true
    });
    ed = editor;
    ed.updateOptions({
        "autoIndent": true,
        "formatOnPaste": true,
        "formatOnType": true
    });
    editor.addListener('didType', () => {
        var val = editor.getValue();

    });

    monaco.languages.register({ id: languageId });

    var langModel = monaco.editor.createModel("return " + JSON.stringify(text), 'javascript');

    var def = null;
    try {
        def = eval("(function(){  " + langModel.getValue() + "; })()");
    } catch (err) {

    }
    //monaco.languages.setMonarchTokensProvider(languageId, def);


    monaco.languages.registerDocumentFormattingEditProvider('csharp', {
        provideDocumentFormattingEdits: function (model, options, token) {
            $('#codeformat').text('code Formatting.');
            var obj = { SourceCode: model.getValue(), Nuget: $('#nugetPacks').val(), lineNumberOffsetFromTemplate: 0 };

            return new Promise((resolve, reject) => {
                $.ajax({
                    url: '/api/compiler/formatCode',
                    data: JSON.stringify(obj),
                    type: 'POST',
                    traditional: true,
                    contentType: 'application/json',
                    success: function (data) {
                        var obj = [
                            {
                                range: {
                                    startLineNumber: 1,
                                    startColumn: 1,
                                    endLineNumber: ed.model.getLineCount(),
                                    endColumn: 100
                                },
                                text: data
                            }
                        ];
                        $('#codeformat').text('code Formatted.');
                        resolve(obj);
                    },
                    error: function (error) {
                        console.log(error)
                    },
                })
            })

        }
    });

});

function getcsharpCompletionProvider(monaco) {

    return {

        triggerCharacters: ['.', ';', '(', ' ', ',', ')'],
        provideCompletionItems: function (model, position) {

            var textUntilPosition = model.getValueInRange({ startLineNumber: 1, startColumn: 1, endLineNumber: position.lineNumber, endColumn: position.column });
            var cursor = textUntilPosition.length;

            var obj = { SourceCode: model.getValue(), Nuget: $('#nugetPacks').val(), lineNumberOffsetFromTemplate: cursor };

            return new Promise((resolve, reject) => {
                $.ajax({
                    url: '/api/compiler/resolve',
                    data: JSON.stringify(obj),
                    type: 'POST',
                    traditional: true,
                    contentType: 'application/json',
                    success: function (data) {
                        var availableResolvers = [];
                        if (data && data.items) {
                            for (var i = 0; i < data.items.length; i++) {
                                if (data.items[i].properties.symbolName) {
                                    var ob = {
                                        label: data.items[i].properties.symbolName,
                                        insertText: data.items[i].properties.symbolName,
                                        kind: data.items[i].properties.symbolKind,// monaco.languages.CompletionItemKind.Property,
                                        detail: data.items[i].tags[0],
                                        documentation: data.items[i].tags[1]
                                    };
                                    availableResolvers.push(ob);
                                } else {
                                    var obj = {
                                        label: data.items[i].displayText,
                                        insertText: data.items[i].displayText,
                                        kind: monaco.languages.CompletionItemKind.Property,
                                        detail: data.items[i].displayText,

                                    };
                                    availableResolvers.push(obj);
                                }


                            }
                            resolve(availableResolvers);
                        } else {
                            $('#buildstatus').text(data);
                        }


                    },
                    error: function (error) {
                        console.log(error)
                    },
                })
            })


        }
    };
}
function testBuild(obj) {

    $.ajax({
        url: '/api/compiler/compile',
        data: JSON.stringify(obj),
        type: 'POST',
        traditional: true,
        contentType: 'application/json',
        success: function (data) {

            if (data && data.indexOf("error") !== -1 && data.indexOf(":") !== -1) {
                var err = data.split(':');
                var lineNum = err[0].split(",")[0].replace('(', '') * 1;
                var colNum = err[0].split(",")[1].replace(')', '') * 1;
                monaco.editor.setModelMarkers(ed.getModel(), 'error', [{
                    startLineNumber: lineNum,
                    startColumn: colNum - 1,
                    endLineNumber: lineNum,
                    endColumn: colNum,
                    message: err[2] + ' ' + err[3],
                    severity: monaco.Severity.Error
                }]);
                $('#buildstatus').text(data);
            } else {
                monaco.editor.setModelMarkers(ed.getModel(), 'error', null);
                $('#buildstatus').text('build valid');
            }

        },
        error: function (error) {
            console.log(error)
        },
    });


}

function getModelValue() {
    return { SourceCode: ed.model.getValue(), Nuget: $('#nugetPacks').val(), lineNumberOffsetFromTemplate: 0 };

}
function getLastLine() {
    return ed.model.getLineCount();
}
function getLastLineContent() {
    return ed.model.getLineContent(ed.model.getLineCount());
}
function insertTextIntoLine(insertText, lineNum) {
    var line = ed.getPosition();
    var range = new monaco.Range(lineNum, lineNum, lineNum, lineNum);
    var id = { major: 1, minor: 1 };
    var text = insertText;
    var op = { identifier: id, range: range, text: text, forceMoveMarkers: true };
    ed.executeEdits("source", [op]);
}
function build() {
    var obj = getModelValue();
    testBuild(obj);
}
function format() {
    ed.trigger(ed.model.getValue(), 'editor.action.formatDocument');
}

                //var interval = setInterval(function () {
                //ed.trigger(ed.model.getValue(), 'editor.action.formatDocument');
                //var obj = getModelValue();
                //testBuild(obj);
                //}, 3000);
