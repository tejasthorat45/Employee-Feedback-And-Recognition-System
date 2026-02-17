import { Component, HostListener } from '@angular/core';
import { RouterLink, Router } from '@angular/router';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-home-page',
  standalone: true,                 // <-- ADD THIS
  imports: [RouterLink, RouterModule, CommonModule],
  templateUrl: './home-page.component.html',
  styleUrls: ['./home-page.component.css'] // <-- Also fix styleUrls
})
export class HomePageComponent {
  showNavButtons = true;
  navButtonClass = '';

  constructor(
    public auth: AuthService,
    private router: Router
  ) {}

  get isLoggedIn$() {
    return this.auth.isLoggedIn$;
  }

  @HostListener('window:scroll')
  onScroll(): void {
    const scrollPosition = window.scrollY;
    const viewportHeight = window.innerHeight;
    
    // Hide nav buttons when in the roles section (third viewport - scroll > 2 * vh)
    this.showNavButtons = scrollPosition < viewportHeight * 2;
    
    // Change button color to fade orange in second viewport (about section)
    if (scrollPosition >= viewportHeight && scrollPosition < viewportHeight * 2) {
      this.navButtonClass = 'fade-orange';
    } else {
      this.navButtonClass = '';
    }
  }

  scrollToAbout(): void {
    const element = document.getElementById('about-anchor');
    if (element) element.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }

  scrollToRoles(): void {
    const element = document.getElementById('roles-anchor');
    if (element) element.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/auth/home-page']);
  }
}