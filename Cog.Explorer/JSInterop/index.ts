import 'typeface-roboto/index.css';
import 'material-design-icons-iconfont/dist/material-design-icons.css';
import 'tabulator-tables/dist/css/materialize/tabulator_materialize.css';

import { TabulatorInterop } from './tabulator-interop';
import { DomInterop } from './dom-interop';

const globals = window as any;
globals.TabulatorInterop = new TabulatorInterop();
globals.DomInterop = new DomInterop();
