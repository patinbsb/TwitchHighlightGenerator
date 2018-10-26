import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import {BrowserAnimationsModule} from '@angular/platform-browser/animations';
import {MatButtonModule, MatInputModule} from '@angular/material';

const routes: Routes = [];

@NgModule({
  imports: [RouterModule.forRoot(routes), BrowserAnimationsModule, MatButtonModule, MatInputModule],
  exports: [RouterModule]
})
export class AppRoutingModule { }
