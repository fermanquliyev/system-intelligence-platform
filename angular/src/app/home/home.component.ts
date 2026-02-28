import { Component, inject } from '@angular/core';
import { AuthService, LocalizationPipe } from '@abp/ng.core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-home',
  template: `
    @if (!hasLoggedIn) {
      <!-- Hero Section -->
      <section class="hero-section py-5 mb-5">
        <div class="container">
          <div class="row align-items-center">
            <div class="col-lg-6">
              <h1 class="display-4 fw-bold mb-4">AI-Powered Error Intelligence for .NET Teams</h1>
              <p class="lead text-muted mb-4">
                Transform your error logs into actionable insights. Get instant AI analysis, smart grouping, and intelligent recommendations to resolve issues faster.
              </p>
              <div class="d-flex gap-3 flex-wrap">
                <button class="btn btn-primary btn-lg px-4" (click)="login()">
                  <i class="fa fa-rocket me-2"></i>Start Free
                </button>
                <button class="btn btn-outline-primary btn-lg px-4" (click)="tryDemo()">
                  <i class="fa fa-play-circle me-2"></i>Try Demo
                </button>
              </div>
            </div>
            <div class="col-lg-6 text-center">
              <div class="hero-image-placeholder p-5 bg-light rounded">
                <i class="fa fa-chart-line fa-5x text-primary"></i>
              </div>
            </div>
          </div>
        </div>
      </section>

      <!-- Features Section -->
      <section class="features-section py-5 mb-5 bg-light">
        <div class="container">
          <div class="text-center mb-5">
            <h2 class="display-5 fw-bold mb-3">Powerful Features</h2>
            <p class="lead text-muted">Everything you need to master your application errors</p>
          </div>
          <div class="row g-4">
            <div class="col-md-6 col-lg-4">
              <div class="card h-100 border-0 shadow-sm">
                <div class="card-body p-4">
                  <div class="feature-icon mb-3">
                    <i class="fa fa-brain fa-3x text-primary"></i>
                  </div>
                  <h4 class="fw-bold mb-3">AI-Powered Analysis</h4>
                  <p class="text-muted">Automatically analyze errors with advanced AI to understand root causes and suggest solutions.</p>
                </div>
              </div>
            </div>
            <div class="col-md-6 col-lg-4">
              <div class="card h-100 border-0 shadow-sm">
                <div class="card-body p-4">
                  <div class="feature-icon mb-3">
                    <i class="fa fa-layer-group fa-3x text-primary"></i>
                  </div>
                  <h4 class="fw-bold mb-3">Smart Grouping</h4>
                  <p class="text-muted">Intelligently group similar errors together to reduce noise and focus on what matters.</p>
                </div>
              </div>
            </div>
            <div class="col-md-6 col-lg-4">
              <div class="card h-100 border-0 shadow-sm">
                <div class="card-body p-4">
                  <div class="feature-icon mb-3">
                    <i class="fa fa-bolt fa-3x text-primary"></i>
                  </div>
                  <h4 class="fw-bold mb-3">Real-Time Monitoring</h4>
                  <p class="text-muted">Get instant notifications when critical errors occur, so you can respond immediately.</p>
                </div>
              </div>
            </div>
            <div class="col-md-6 col-lg-4">
              <div class="card h-100 border-0 shadow-sm">
                <div class="card-body p-4">
                  <div class="feature-icon mb-3">
                    <i class="fa fa-search fa-3x text-primary"></i>
                  </div>
                  <h4 class="fw-bold mb-3">Advanced Search</h4>
                  <p class="text-muted">Search through errors using natural language queries powered by semantic search.</p>
                </div>
              </div>
            </div>
            <div class="col-md-6 col-lg-4">
              <div class="card h-100 border-0 shadow-sm">
                <div class="card-body p-4">
                  <div class="feature-icon mb-3">
                    <i class="fa fa-chart-bar fa-3x text-primary"></i>
                  </div>
                  <h4 class="fw-bold mb-3">Analytics & Insights</h4>
                  <p class="text-muted">Track error trends, identify patterns, and measure improvement over time.</p>
                </div>
              </div>
            </div>
            <div class="col-md-6 col-lg-4">
              <div class="card h-100 border-0 shadow-sm">
                <div class="card-body p-4">
                  <div class="feature-icon mb-3">
                    <i class="fa fa-shield-alt fa-3x text-primary"></i>
                  </div>
                  <h4 class="fw-bold mb-3">Enterprise Security</h4>
                  <p class="text-muted">Bank-level security with encryption, audit logs, and compliance certifications.</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      <!-- Pricing Section -->
      <section class="pricing-section py-5 mb-5">
        <div class="container">
          <div class="text-center mb-5">
            <h2 class="display-5 fw-bold mb-3">Simple, Transparent Pricing</h2>
            <p class="lead text-muted">Choose the plan that fits your team</p>
          </div>
          <div class="row g-4 justify-content-center">
            <!-- Free Plan -->
            <div class="col-lg-4 col-md-6">
              <div class="card h-100 border shadow-sm">
                <div class="card-body p-4">
                  <h3 class="fw-bold mb-2">Free</h3>
                  <div class="mb-4">
                    <span class="display-4 fw-bold">$0</span>
                    <span class="text-muted">/month</span>
                  </div>
                  <ul class="list-unstyled mb-4">
                    <li class="mb-3"><i class="fa fa-check text-success me-2"></i>Up to 1,000 errors/month</li>
                    <li class="mb-3"><i class="fa fa-check text-success me-2"></i>Basic AI analysis</li>
                    <li class="mb-3"><i class="fa fa-check text-success me-2"></i>Email support</li>
                    <li class="mb-3"><i class="fa fa-check text-success me-2"></i>1 application</li>
                  </ul>
                  <button class="btn btn-outline-primary w-100" (click)="login()">Get Started</button>
                </div>
              </div>
            </div>
            <!-- Pro Plan -->
            <div class="col-lg-4 col-md-6">
              <div class="card h-100 border-primary shadow-lg position-relative">
                <div class="position-absolute top-0 start-50 translate-middle">
                  <span class="badge bg-primary px-3 py-2">Most Popular</span>
                </div>
                <div class="card-body p-4">
                  <h3 class="fw-bold mb-2">Pro</h3>
                  <div class="mb-4">
                    <span class="display-4 fw-bold">$49</span>
                    <span class="text-muted">/month</span>
                  </div>
                  <ul class="list-unstyled mb-4">
                    <li class="mb-3"><i class="fa fa-check text-success me-2"></i>Up to 50,000 errors/month</li>
                    <li class="mb-3"><i class="fa fa-check text-success me-2"></i>Advanced AI analysis</li>
                    <li class="mb-3"><i class="fa fa-check text-success me-2"></i>Priority support</li>
                    <li class="mb-3"><i class="fa fa-check text-success me-2"></i>Unlimited applications</li>
                    <li class="mb-3"><i class="fa fa-check text-success me-2"></i>Custom integrations</li>
                  </ul>
                  <button class="btn btn-primary w-100" (click)="login()">Start Free Trial</button>
                </div>
              </div>
            </div>
            <!-- Enterprise Plan -->
            <div class="col-lg-4 col-md-6">
              <div class="card h-100 border shadow-sm">
                <div class="card-body p-4">
                  <h3 class="fw-bold mb-2">Enterprise</h3>
                  <div class="mb-4">
                    <span class="display-4 fw-bold">Custom</span>
                  </div>
                  <ul class="list-unstyled mb-4">
                    <li class="mb-3"><i class="fa fa-check text-success me-2"></i>Unlimited errors</li>
                    <li class="mb-3"><i class="fa fa-check text-success me-2"></i>Premium AI features</li>
                    <li class="mb-3"><i class="fa fa-check text-success me-2"></i>24/7 dedicated support</li>
                    <li class="mb-3"><i class="fa fa-check text-success me-2"></i>SLA guarantee</li>
                    <li class="mb-3"><i class="fa fa-check text-success me-2"></i>Custom deployment</li>
                  </ul>
                  <button class="btn btn-outline-primary w-100" (click)="login()">Contact Sales</button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>
    } @else {
      <!-- Logged in users see dashboard redirect -->
      <div class="container py-5">
        <div class="row justify-content-center">
          <div class="col-md-6 text-center">
            <h2 class="mb-4">Welcome back!</h2>
            <p class="lead text-muted mb-4">Redirecting to your dashboard...</p>
            <a routerLink="/dashboard" class="btn btn-primary">Go to Dashboard</a>
          </div>
        </div>
      </div>
    }
  `,
  styles: [`
    .hero-section {
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
      border-radius: 0 0 50px 50px;
    }
    .hero-section h1 {
      color: white;
    }
    .hero-section .text-muted {
      color: rgba(255, 255, 255, 0.9) !important;
    }
    .hero-image-placeholder {
      background: rgba(255, 255, 255, 0.1) !important;
      backdrop-filter: blur(10px);
    }
    .hero-image-placeholder i {
      color: white;
    }
    .features-section .card {
      transition: transform 0.3s ease, box-shadow 0.3s ease;
    }
    .features-section .card:hover {
      transform: translateY(-5px);
      box-shadow: 0 0.5rem 1rem rgba(0, 0, 0, 0.15) !important;
    }
    .feature-icon {
      height: 60px;
    }
    .pricing-section .card {
      transition: transform 0.3s ease;
    }
    .pricing-section .card:hover {
      transform: translateY(-5px);
    }
    .pricing-section .badge {
      margin-top: -15px;
    }
  `],
  imports: [LocalizationPipe, CommonModule, RouterLink]
})
export class HomeComponent {
  private authService = inject(AuthService);

  get hasLoggedIn(): boolean {
    return this.authService.isAuthenticated;
  }

  login() {
    this.authService.navigateToLogin();
  }

  tryDemo() {
    // Navigate to demo or show demo modal
    this.authService.navigateToLogin();
  }
}
