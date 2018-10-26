import { Component } from '@angular/core';
import {MatButtonModule} from '@angular/material';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'Twitch Highlight Reel Generator';

  onSubmit() {
    // Here we do the vod download and all the other stuff, keeping the user informed of steps
    // Then we direct to new page with outcome, some stats, and eiother highlight vid download or embedded video.
    console.log('success');
  }
}
