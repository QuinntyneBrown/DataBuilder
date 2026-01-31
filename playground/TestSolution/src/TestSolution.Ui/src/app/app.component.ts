import { Component } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatSidenavModule,
    MatListModule
  ],
  template: `
    <div class="nav-container">
      <mat-toolbar color="primary" class="main-toolbar">
        <button mat-icon-button (click)="sidenav.toggle()">
          <mat-icon>menu</mat-icon>
        </button>
        <span>TestSolution</span>
      </mat-toolbar>

      <mat-sidenav-container class="content">
        <mat-sidenav #sidenav mode="side" opened>
          <mat-nav-list>
            <a mat-list-item routerLink="/customers" routerLinkActive="active">
              <mat-icon matListItemIcon>list</mat-icon>
              <span matListItemTitle>Customers</span>
            </a>
                      <a mat-list-item routerLink="/products" routerLinkActive="active">
              <mat-icon matListItemIcon>list</mat-icon>
              <span matListItemTitle>Products</span>
            </a>

          </mat-nav-list>
        </mat-sidenav>

        <mat-sidenav-content>
          <router-outlet></router-outlet>
        </mat-sidenav-content>
      </mat-sidenav-container>
    </div>
  `,
  styles: [`
    .nav-container {
      display: flex;
      flex-direction: column;
      height: 100vh;
    }
    mat-sidenav-container {
      flex: 1;
    }
    mat-sidenav {
      width: 240px;
    }
    .active {
      background: rgba(255, 255, 255, 0.1);
    }
  `]
})
export class AppComponent {
  title = 'TestSolution';
}
