import Tabulator from 'tabulator-tables';

const AUDIO_BUTTON_HTML = `
  <button class="mdc-icon-button audio-button" aria-hidden="true" aria-pressed="false">
    <i class="material-icons mdc-icon-button__icon mdc-icon-button__icon--on">stop</i>
    <i class="material-icons mdc-icon-button__icon">play_arrow</i>
  </button>
`;

const ICON_BUTTON_ON = 'mdc-icon-button--on';

interface AudioSegment {
  url: string;
  startOffset: number;
  endOffset: number;
}

export class Table {
  private readonly table: Tabulator;
  private readonly audio = new Audio();
  private timeUpdateListener?: (evt: Event) => any;
  private playingElement?: HTMLElement;
  private lastPcnt: number = 0;
  private timer?: number;

  constructor(id: string, columns: any[], data: any) {
    const columnDefs = this.getColumnDefs(columns);
    this.table = new Tabulator(`#${id}`, {
      height: '100%',
      selectable: false,
      columns: columnDefs,
      data,
      layoutColumnsOnNewData: true,
      columnVertAlign: 'bottom'
    });
  }

  setColumns(columns: any[]): void {
    const columnDefs = this.getColumnDefs(columns);
    this.table.setColumns(columnDefs);
  }

  setData(data: any): void {
    this.table.setData(data);
  }

  setPlaybackRate(rate: number): void {
    this.audio.defaultPlaybackRate = rate;
    this.audio.playbackRate = rate;
  }

  private getColumnDefs(columns: any[]): Tabulator.ColumnDefinition[] {
    for (let i = 0; i < columns.length; i++) {
      const column = columns[i];
      if (column.formatter === 'audio') {
        column.formatter = () => AUDIO_BUTTON_HTML;
        column.cellClick = (_e: any, cell: Tabulator.CellComponent) => {
          const elem = cell.getElement();
          const buttonElem = elem.firstElementChild!;
          const isOn = buttonElem.classList.contains(ICON_BUTTON_ON);
          if (isOn) {
            buttonElem.classList.remove(ICON_BUTTON_ON);
            this.stopAudio();
          } else {
            if (this.playingElement != null) {
              this.playingElement.firstElementChild!.classList.remove(ICON_BUTTON_ON);
              this.stopAudio();
            }
            buttonElem.classList.add(ICON_BUTTON_ON);
            const audioSegment: AudioSegment = cell.getRow().getData()[cell.getField()];
            this.playAudio(audioSegment.url, audioSegment.startOffset, audioSegment.endOffset);
            this.playingElement = elem;
          }
          elem.setAttribute('aria-pressed', `${!isOn}`)
        };
      }
    }
    return columns;
  }

  private playAudio(url: string, start: number, end: number): void {
    this.stopAudio();

    this.audio.src = url;
    this.audio.currentTime = start;
    this.timer = setInterval(() => {
      if (this.playingElement == null) {
        return;
      }

      if (this.audio.currentTime >= end) {
        this.audio.currentTime = start;
        if (this.audio.paused) {
          this.audio.play();
        }
      }
      const pcnt = Math.round(((this.audio.currentTime - start) / (end - start)) * 100);
      if (pcnt < this.lastPcnt) {
        this.playingElement.classList.remove('animate-progress');
      } else {
        this.playingElement.classList.add('animate-progress');
      }
      this.playingElement.style.backgroundSize = `${pcnt}% 100%`;
      this.lastPcnt = pcnt;
    }, 250);
    //this.timeUpdateListener = () => {
    //  if (this.playingElement == null) {
    //    return;
    //  }

    //  if (this.audio.currentTime >= end) {
    //    this.audio.currentTime = start;
    //    if (this.audio.paused) {
    //      this.audio.play();
    //    }
    //  }
    //  const pcnt = Math.round(((this.audio.currentTime - start) / (end - start)) * 100);
    //  if (pcnt < this.lastPcnt) {
    //    this.playingElement.classList.remove('animate-progress');
    //  } else {
    //    this.playingElement.classList.add('animate-progress');
    //  }
    //  this.playingElement.style.backgroundSize = `${pcnt}% 100%`;
    //  this.lastPcnt = pcnt;
    //};
    //this.audio.addEventListener('timeupdate', this.timeUpdateListener);

    this.audio.play();
  }

  private stopAudio(): void {
    if (this.playingElement != null) {
      this.playingElement.classList.remove('animate-progress');
      this.playingElement.style.backgroundSize = '';
    }
    this.lastPcnt = 0;
    //if (this.timeUpdateListener != null) {
    //  this.audio.removeEventListener('timeupdate', this.timeUpdateListener);
    //}
    if (this.timer != null) {
      clearInterval(this.timer);
    }
    if (!this.audio.paused) {
      this.audio.pause();
    }
  }
}