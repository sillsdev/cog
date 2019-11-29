import 'typeface-roboto/index.css';
import './site.scss';

import { TabulatorInterop } from './tabulator-interop';
import { DomInterop } from './dom-interop';

const globals = window as any;
globals.TabulatorInterop = new TabulatorInterop();
globals.DomInterop = new DomInterop();
